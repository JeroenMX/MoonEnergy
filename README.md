# MoonEnergy

## Description

MoonEnergy is a fictitious company selling moon energy and this solution contains an example AI Chatbot. 

## Features

- Chat interface for interacting with OpenAI's API.
- Ability to get weather information. 'What's the weather in Amsterdam?'
- Manage user information such as "Installment amount" or "Termijnbedrag".
Examples: 'What is my installment amount?', 'Wat is mijn termijnbedrag', 'Change my installment amount' or 'Wijzig mijn termijnbdrag'
- Backend REST API services for handling chat functionality.
- Render json as part of the response.

## Getting Started

1. Obtain your Open AI API key (see Configuration)
2. Start both projects and frontend app
3. When prompted to login use alice:alice or bob:bob as credentials. (can be changed in MoonEnergy.Sso/Pages/TestUsers.cs)

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for frontend development)
- [OpenAI API Key](https://beta.openai.com/signup)

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/your-repository.git
    cd your-repository/src/MoonEnergy/MoonEnergy
    ```

2. Install frontend dependencies:
    ```sh
    cd clientapp
    npm install
    ```

3. Restore backend dependencies:
    ```sh
    cd ..
    dotnet restore
    ```

### Configuration

1. **OpenAI API Key:**

   To integrate with OpenAI, you will need an API key. Follow these steps to obtain it:

    - Go to [OpenAI's API](https://beta.openai.com/signup/).
    - Sign up for an account or log in if you already have one.
    - Navigate to the API keys section in your account settings.
    - Create a new API key.
    - Copy the API key and paste it into your project's configuration file or environment variables.

2. **Configure API Key:**

   Add the OpenAI API key to your `appsettings.json` or better `appsettings.local.json` file:
    ```json
    {
      "OpenAI": {
        "ApiKey": "your_openai_api_key_here"
      }
    }
    ```

### Running the Application

1. Run the backend:
    ```sh
    dotnet run
    ```

   The backend server will start running on `https://localhost:5001` or another URL specified in your configuration.

2. Run the frontend:
    ```sh
    cd clientapp
    npm run dev
    ```

   The frontend development server will start, typically on `http://localhost:5173`.

### Building the Application

To build the application for production:

1. Build the backend:
    ```sh
    dotnet build --configuration Release
    ```

2. Build the frontend:
    ```sh
    cd clientapp
    npm run build
    ```