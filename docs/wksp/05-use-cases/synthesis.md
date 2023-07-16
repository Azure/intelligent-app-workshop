# Synthesizing and Summarizing private data to generate recommendations and insights in your Applications

In this section, we delve into the process of synthesizing portfolio recommendations as actionable insights using Project Miyagi. The workflow for this is orchestrated by the Semantic Kernel, as illustrated below:

![sk-orchestration](../../assets/images/sk-memory-orchestration.png)

??? tip
    For advanced deep learning-based recommender systems, checout [microsoft/recommenders](https://github.com/microsoft/recommenders#algorithms) for complex DLRMs. 

## Initializing the Kernel

Firstly, we initialize the Kernel following the method [outlined in Project Miyagi](https://github.com/Azure-Samples/miyagi/blob/main/dotnet/recommendation-service/Program.cs#L30-L59). 

=== "C#"

    ```csharp hl_lines="1-1 3 5 11" linenums="1"
    var kernel = new KernelBuilder()
        .WithLogger(NullLogger.Instance)
        .WithCompletionService(kernelSettings)
        .WithEmbeddingGenerationService(kernelSettings)
        .WithMemoryStorage(memoryStore)
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
    kernel.config.add_chat_completion_service("dv",
                    OpenAIChatCompletion("gpt-35-turbo",
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

    ```csharp hl_lines="5 9 12 18 27" linenums="1"
    // ========= Import Advisor skill from local filesystem =========

        // ========= Import semantic functions as plugins =========
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
        var advisorPlugin = _kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "AdvisorPlugin");
        
        // This will explained in full later in this section:
         // ========= Import native function  =========
        var userProfilePlugin = _kernel.ImportSkill(new UserProfilePlugin(), "UserProfilePlugin");

        // ========= Set context variables to populate the prompt  =========
        var context = _kernel.CreateNewContext();
        context.Variables.Set("userId", miyagiContext.UserInfo.UserId);
        context.Variables.Set("portfolio", JsonSerializer.Serialize(miyagiContext.Portfolio));
        context.Variables.Set("risk", miyagiContext.UserInfo.RiskLevel ?? DefaultRiskLevel);

        // ========= Chain and orchestrate with LLM =========
        var plan = new Plan("Execute userProfilePlugin and then advisorPlugin");
        plan.AddSteps(userProfilePlugin["GetUserAge"],
            userProfilePlugin["GetAnnualHouseholdIncome"],
            advisorPlugin["PortfolioAllocation"]);

        // Execute plan
        var ask = "Using the userId, get user age and household income, and then get the recommended asset allocation";
        context.Variables.Update(ask);
        log?.LogDebug("Planner steps: {N}", plan.Steps.Count);
        var result = await plan.InvokeAsync(context);
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



### Native functions with RunAsync

!!! Tip "Use planner whenever possible"
    With 4 types of planners, you can easily chain and orchestrate your skills. However, if there are times when planner might not suit your needs, you could revert to using `RunAsync` to call a native function. 


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
        // Copyright (c) Microsoft. All rights reserved.

        using System.ComponentModel;
        using Microsoft.SemanticKernel.Orchestration;
        using Microsoft.SemanticKernel.SkillDefinition;

        namespace GBB.Miyagi.RecommendationService.plugins;

        /// <summary>
        ///     UserProfilePlugin shows a native skill example to look user info given userId.
        /// </summary>
        /// <example>
        ///     Usage: kernel.ImportSkill("UserProfilePlugin", new UserProfilePlugin());
        ///     Examples:
        ///     SKContext["userId"] = "000"
        /// </example>
        public class UserProfilePlugin
        {
            /// <summary>
            ///     Name of the context variable used for UserId.
            /// </summary>
            public const string UserId = "UserId";

            private const string DefaultUserId = "40";
            private const int DefaultAnnualHouseholdIncome = 150000;
            private const int Normalize = 81;

            /// <summary>
            ///     Lookup User's age for a given UserId.
            /// </summary>
            /// <example>
            ///     SKContext[UserProfilePlugin.UserId] = "000"
            /// </example>
            /// <param name="context">Contains the context variables.</param>
            [SKFunction]
            [SKName("GetUserAge")]
            [Description("Given a userId, get user age")]
            public string GetUserAge(
                [Description("Unique identifier of a user")]
                string userId,
                SKContext context)
            {
                // userId = context.Variables.ContainsKey(UserId) ? context[UserId] : DefaultUserId;
                userId = string.IsNullOrEmpty(userId) ? DefaultUserId : userId;
                context.Log.LogDebug("Returning hard coded age for {0}", userId);

                int age;

                if (int.TryParse(userId, out var parsedUserId))
                    age = parsedUserId > 100 ? parsedUserId % Normalize : parsedUserId;
                else
                    age = int.Parse(DefaultUserId);

                // invoke a service to get the age of the user, given the userId
                return age.ToString();
            }

            /// <summary>
            ///     Lookup User's annual income given UserId.
            /// </summary>
            /// <example>
            ///     SKContext[UserProfilePlugin.UserId] = "000"
            /// </example>
            /// <param name="context">Contains the context variables.</param>
            [SKFunction]
            [SKName("GetAnnualHouseholdIncome")]
            [Description("Given a userId, get user annual household income")]
            public string GetAnnualHouseholdIncome(
                [Description("Unique identifier of a user")]
                string userId,
                SKContext context)
            {
                // userId = context.Variables.ContainsKey(UserId) ? context[UserId] : DefaultUserId;
                userId = string.IsNullOrEmpty(userId) ? DefaultUserId : userId;
                context.Log.LogDebug("Returning userId * randomMultiplier for {0}", userId);

                var random = new Random();
                var randomMultiplier = random.Next(1000, 8000);

                // invoke a service to get the annual household income of the user, given the userId
                var annualHouseholdIncome = int.TryParse(userId, out var parsedUserId)
                    ? parsedUserId * randomMultiplier
                    : DefaultAnnualHouseholdIncome;

                return annualHouseholdIncome.ToString();
            }
        }
    ```
??? tip "This is how you prompt engineer in your code"
    The snippets above highlight some of the primitives of SK that allow you to easily integrate `prompt engineering` in your existing workflows.
    
More coming soon...

[Sign up for updates](https://forms.office.com/r/rLds2s8RH1){ :target="_blank" .md-button .md-button--primary }