# Toluwaloope Location Detector

This project is an Azure Function App that provides an API endpoint to detect the caller's geolocation based on their IP address. It uses the ip-api.com service to retrieve location details such as city, region, country, latitude, and longitude.

## Features
- Azure Function App (Isolated .NET 8)
- API endpoint: `/api/GetCallerInformation`
- Returns caller's geolocation (city, region, country, zip, lat, lon)
- Uses ip-api.com for IP geolocation lookup
- Organized codebase with Models, Services, and Functions
- Infrastructure as Code with Terraform
- Automated CI/CD with GitHub Actions

## How to Deploy with GitHub Actions

The project includes a reusable workflow for automated deployment to Azure. The main workflow is `.github/workflows/deploy-function-app.yml`.

### Steps:
1. **Configure Secrets**
   - Set the following secrets in your GitHub repository:
     - `AZURE_CLIENT_ID` (Service Principal Client ID)
     - `AZURE_TENANT_ID` (Azure Tenant ID)
     - `AZURE_SUBSCRIPTION_ID` (Azure Subscription ID)

2. **Trigger the Workflow**
   - Go to the Actions tab in GitHub and run the `Build and Deploy to Toluwaloope.LocationDetector.Api Azure Function App` workflow.
   - Choose the environment (`dev` or `prod`).

3. **Workflow Overview**
   - **provision-infrastructure**: Provisions Azure resources using Terraform. Uses dev subscription for backend state, and target subscription for resource deployment.
   - **build**: Builds and publishes the Function App.
   - **deploy**: Deploys the app to the selected environment.
   - **post-deployment-tests**: Runs health checks on the deployed API endpoint.
   - **notify**: Summarizes deployment status.

### API Usage
After deployment, you can call the API endpoint:

```
GET https://<function-app-name>.azurewebsites.net/api/GetCallerInformation
```

The response will include geolocation details for the caller's IP address.

## Local Development
- Code is in `src/Toluwaloope.LocationDetector.Api`
- Infrastructure code is in `infrastructure/terraform`
- To build locally:
  ```sh
  dotnet build src/Toluwaloope.LocationDetector.Api
  ```
- To run locally: Follow this [Azure guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-local?pivots=programming-language-csharp)
  ```sh
  func start
  ```

## License
MIT
