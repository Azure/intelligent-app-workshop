# Prerequisites

!!! note "Mandatory pre-requisite"

    Please signup for [Azure OpenAI (AOAI)](https://aka.ms/oai/access) and complete [Getting started with AOAI module](https://learn.microsoft.com/en-us/training/modules/get-started-openai/)

Before attending the Intelligent App Development Workshop, please ensure you have the following prerequisites in place:

1. **Basic programming knowledge**: Familiarity with at least one programming language (e.g., Python, JavaScript, Java, or C#) and basic understanding of software development concepts.
1. **Azure account**: A Microsoft Azure account with an active subscription. If you don't have one, sign up for a [free trial](https://azure.microsoft.com/en-us/free/).
1. **Azure subscription with access enabled for the Azure OpenAI Service** - For more details, see the [Azure OpenAI Service documentation on how to get access](https://learn.microsoft.com/azure/ai-services/openai/overview#how-do-i-get-access-to-azure-openai). 
1. **Azure OpenAI resource** - For this workshop, you'll need to deploy at least one model such as GPT 4. See the Azure OpenAI Service documentation for more details on [deploying models](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource?pivots=web-portal) and [model availability](https://learn.microsoft.com/azure/ai-services/openai/concepts/models).
1. **Development environment**: A computer with your preferred development environment installed, such as Visual Studio Code, PyCharm, or another IDE that supports the programming language you'll be using in the workshop.
1. **Git**: Familiarity with Git and GitHub for version control. Ensure you have [Git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git) installed on your computer.
1. **.NET CLI**: .NET CLI is included when you install [.NET SDK](https://dotnet.microsoft.com/en-us/download)

## Initial Setup

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed.
1. Clone this repo using: 

    ```bash
    git clone https://github.com/Azure/intelligent-app-workshop.git
    ```

1. Change directory into cloned repo:

    ```bash
    cd intelligent-app-workshop
    ```

1. Copy and rename the file `appsettings.json.example` into the corresponding lesson directory as follows (example command for Lesson1):

    ```bash
    cp workshop\donet\Lessons\appsettings.json.example workshop\dotnet\Lessons\Lesson1\appsettings.json
    ```

1. Retrieve the OpenAI Endpoint URL, deployed model name and API Key (from pre-requisites) into the app settings


??? note "Optional"
    The following prerequisites are optional but recommended to get the most out of the workshop:
    
    1. **Azure CLI**: Install the [Azure Command-Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) to interact with Azure services and manage resources from the command line.

    1. **Azure Functions Core Tools**: Install the [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash#v2) to create, test, and deploy Azure Functions from your local development environment.
    
    1. **Docker**: Install [Docker Desktop](https://www.docker.com/products/docker-desktop) to build and run containerized applications.
    
    1. **Cognitive Services SDKs**: Install the SDKs for the Azure Cognitive Services you'll be using in the workshop, such as the [Azure OpenAI SDK](https://pypi.org/project/azure-cognitiveservices-openai/), based on your programming language and the services used during the workshop.

By ensuring you have completed these prerequisites, you'll be well-prepared to dive into the Intelligent App Development Workshop and make the most of the hands-on learning experience.
