# Synthesizing and Summarizing private data to generate recommendations and insights in your Applications

In this section, we delve into the process of synthesizing portfolio recommendations as actionable insights using Project Miyagi. The workflow for this is orchestrated by the Semantic Kernel, as illustrated below:

![sk-orchestration](../../assets/images/sk-memory-orchestration.png)

??? tip
    For advanced deep learning-based recommender systems, checout [microsoft/recommenders](https://github.com/microsoft/recommenders#algorithms) for complex DLRMs. 

## Initializing the Kernel

Firstly, we initialize the Kernel following the method [outlined in Project Miyagi](https://github.com/Azure-Samples/miyagi/blob/main/dotnet/recommendation-service/Program.cs#L30-L59). 

=== "C#"

    ```csharp hl_lines="1-1 4 9 16" linenums="1"
    var kernel = Kernel.Builder
        .WithLogger(ConsoleLogger.Log)
        .Configure(c =>
        {
            c.AddAzureTextCompletionService(
                Env.Var("AZURE_OPENAI_SERVICE_ID"),
                Env.Var("AZURE_OPENAI_DEPLOYMENT_NAME"),
                Env.Var("AZURE_OPENAI_ENDPOINT"),
                Env.Var("AZURE_OPENAI_KEY"));
            c.AddAzureTextEmbeddingGenerationService(
                Env.Var("AZURE_OPENAI_EMBEDDINGS_SERVICE_ID"),
                Env.Var("AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT_NAME"),
                Env.Var("AZURE_OPENAI_EMBEDDINGS_ENDPOINT"),
                Env.Var("AZURE_OPENAI_EMBEDDINGS_KEY"));
        })
        .WithMemoryStorage(new VolatileMemoryStore())
        .Configure(c => c.SetDefaultHttpRetryConfig(new HttpRetryConfig
        {
            MaxRetryCount = 2,
            UseExponentialBackoff = true
        }))
        .Build();
    ```
=== "Python"

    ```python hl_lines="4 7-8 11-12 16" linenums="1"
    import semantic_kernel as sk
    from semantic_kernel.connectors.ai.open_ai import AzureTextCompletion

    kernel = sk.Kernel()

    api_key, org_id = sk.openai_settings_from_dot_env()
    kernel.config.add_text_completion_service("dv",
                    OpenAITextCompletion("text-davinci-003",
                                            api_key,
                                            org_id))
    kernel.config.add_text_embedding_generation_service("ada",
                    OpenAITextEmbedding("text-embedding-ada-002",
                                            api_key,
                                            org_id))

    kernel.register_memory_store(memory_store=sk.memory.VolatileMemoryStore())
    ```
!!! note 
    We are utilizing both the Embeddings and Completion services as both models are integral to the process depicted above.

## Semantic Skills (prompts and prompt engineering)

After we initialize the Kernel, we can now begin to create the prompts that will be used to generate the synthesized insights. This can be more art than science, and we encourage you to experiment with different prompts and configurations to see what works best for your use case.

For the purposes of this workshop, we are envisioning a use case where you select a portfolio of stocks, and the system generates a summary of the portfolio, along with a recommendation on whether to buy, sell, or hold the portfolio.

=== "C#"

    ```csharp hl_lines="5 12" linenums="1"
    // ========= Import Advisor skill from local filesystem =========

        // You can also import this from Azure Blob Storage
        var skillsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Skills");
        var advisorSkill = _kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "AdvisorSkill");
        
        // This will explained in full later in this section:
        var result = await _kernel.RunAsync(
            context,
            userProfileSkill["GetUserAge"],
            userProfileSkill["GetAnnualHouseholdIncome"],
            advisorSkill["InvestmentAdvise"]);
    ```
=== "Investment advise skill"

    ```json hl_lines="8 15-21" linenums="1"
     --8<-- "docs/assets/Skills/AdvisorSkill/InvestmentAdvise/skprompt.txt"
    +++++
    ```
=== "Configuration for advisor skill"

    ```json hl_lines="3 6-8" linenums="1"
    --8<-- "docs/assets/Skills/AdvisorSkill/InvestmentAdvise/config.json"
    +++++
    ```
??? tip "This is how you prompt engineer in your code"
    The snippets above highlight some of the primitives of SK that allow you to easily integrate `prompt engineering` in your existing workflows.


### Native functions

=== "C# Skill usage"

    ```csharp hl_lines="1 6-7" linenums="1"
    var userProfileSkill = _kernel.ImportSkill(new UserProfileSkill(), "UserProfileSkill");

    // ========= Orchestrate with LLM using context, connector, and memory =========
        var result = await _kernel.RunAsync(
            context,
            userProfileSkill["GetUserAge"],
            userProfileSkill["GetAnnualHouseholdIncome"],
            advisorSkill["InvestmentAdvise"]);
        _kernel.Log.LogDebug("Result: {0}", result.Result);
    ```
=== "C# Skill implementation"

    ```csharp linenums="1"
        public class UserProfileSkill
        {
            /// <summary>
            ///     Name of the context variable used for UserId.
            /// </summary>
            public const string UserId = "UserId";
        
            private const string DefaultUserId = "50";
            private const int DefaultAnnualHouseholdIncome = 150000;
        
            /// <summary>
            ///     Lookup User's age for a given UserId.
            /// </summary>
            /// <example>
            ///     SKContext[UserProfileSkill.UserId] = "000"
            /// </example>
            /// <param name="context">Contains the context variables.</param>
            [SKFunction("Given a userId, find user age")]
            [SKFunctionName("GetUserAge")]
            [SKFunctionContextParameter(Name = UserId, Description = "UserId", DefaultValue = DefaultUserId)]
            public string GetUserAge(SKContext context)
            {
                var userId = context.Variables.ContainsKey(UserId) ? context[UserId] : DefaultUserId;
                context.Log.LogDebug("Returning hard coded age for {0}", userId);
        
                int parsedUserId;
                int age;
        
                if (int.TryParse(userId, out parsedUserId))
                {
                    age = parsedUserId > 100 ? 20 + (parsedUserId % 81) : parsedUserId;
                }
                else
                {
                    age = int.Parse(DefaultUserId);
                }
        
                // invoke a service to get the age of the user, given the userId
                return age.ToString();
            }
            
            /// <summary>
            ///     Lookup User's annual income given UserId.
            /// </summary>
            /// <example>
            ///     SKContext[UserProfileSkill.UserId] = "000"
            /// </example>
            /// <param name="context">Contains the context variables.</param>
            [SKFunction("Given a userId, find user age")]
            [SKFunctionName("GetAnnualHouseholdIncome")]
            [SKFunctionContextParameter(Name = UserId, Description = "UserId", DefaultValue = DefaultUserId)]
            public string GetAnnualHouseholdIncome(SKContext context)
            {
                var userId = context.Variables.ContainsKey(UserId) ? context[UserId] : DefaultUserId;
                context.Log.LogDebug("Returning userId * randomMultiplier for {0}", userId);
        
                var random = new Random();
                var randomMultiplier = random.Next(1000, 8000);
        
                // invoke a service to get the annual household income of the user, given the userId
                var annualHouseholdIncome = int.TryParse(userId, out var parsedUserId)
                    ? parsedUserId * randomMultiplier
                    : DefaultAnnualHouseholdIncome;
        
                return annualHouseholdIncome.ToString();
            }
    ```

More coming soon...

[Sign up for updates](https://forms.office.com/r/rLds2s8RH1){ :target="_blank" .md-button .md-button--primary }