# Secure Aggregation API

## Overview

This project demonstrates a simplified secure aggregation system involving three simulated patients (Alice, Bob, and Charlie) and a Hospital. Each patient has a numerical secret (e.g., a health metric). Patients submit their secrets, which are then split into additive shares and distributed among the participants. The hospital can then request the aggregated sum of all patient secrets without knowing any individual patient's original secret. This is achieved by summing the shares collected by each participant. The goal is to show how data can be aggregated securely, ensuring that individual contributions remain private.

The project is built using ASP.NET Core and includes API endpoints for patients to submit data and for the hospital to retrieve aggregated results, protected by API key authentication.

## Prerequisites

*   .NET 7.0 SDK or later

## Getting Started

1.  **Clone the repository / Open the project:**
    Ensure you have the project files in your local workspace.
2.  **Navigate to the project directory:**
    Open your terminal and change to the root directory of the `SecureAggregationAPI` project (where the `.csproj` file is located).
3.  **Run the application:**
    ```sh
    dotnet run
    ```
4.  The API will typically be available at `http://localhost:5000` and `https://localhost:5001`. Check your terminal output for the exact ports.

## How It Works

The core idea is to use additive secret sharing.

1.  **Patient Data Submission:**
    *   Each patient (Alice, Bob, or Charlie) has a secret integer value they wish to contribute.
    *   When a patient submits their value (e.g., `S_A` for Alice) via the `POST /api/patients/submit-share` endpoint:
        *   The patient's original secret is stored (e.g., in [`aliceShares`](Controllers/PatientsController.cs)).
        *   The [`SecretSharingService.Split(secret, 3)`](Services/SecretSharingService.cs) method divides this secret `S_A` into three additive shares: `s_A1, s_A2, s_A3` such that `s_A1 + s_A2 + s_A3 = S_A`.
        *   These shares are then distributed as follows (using Alice's submission of `S_A` as an example, where `randomShares = [s_A1, s_A2, s_A3]`):
            *   `bobCollectedShares` receives `s_A1` (a share of Alice's secret goes to Bob).
            *   `charlieCollectedShares` receives `s_A2` (a share of Alice's secret goes to Charlie).
            *   `aliceCollectedShares` receives `s_A3` (Alice keeps one share of her own secret).
        *   This process is repeated for Bob and Charlie when they submit their secrets (`S_B` and `S_C`).

2.  **Collected Shares:**
    After all three patients have submitted their data, the `collectedShares` lists will look like this:
    *   [`aliceCollectedShares`](Controllers/PatientsController.cs): `[s_A_keptByAlice, s_B_fromBob, s_C_fromCharlie]`
    *   [`bobCollectedShares`](Controllers/PatientsController.cs): `[s_A_fromAlice, s_B_keptByBob, s_C_fromCharlie]`
    *   [`charlieCollectedShares`](Controllers/PatientsController.cs): `[s_A_fromAlice, s_B_fromBob, s_C_keptByCharlie]`

3.  **Hospital Aggregation:**
    *   The hospital uses the `GET /api/patients/get-aggregate-value` endpoint (which requires an API key).
    *   The API calculates the sum of shares in each `collectedShares` list:
        *   `sum_AliceCollected = s_A_keptByAlice + s_B_fromBob + s_C_fromCharlie`
        *   `sum_BobCollected = s_A_fromAlice + s_B_keptByBob + s_C_fromCharlie`
        *   `sum_CharlieCollected = s_A_fromAlice + s_B_fromBob + s_C_keptByCharlie`
    *   The total aggregate value is `sum_AliceCollected + sum_BobCollected + sum_CharlieCollected`.
    *   By rearranging the terms, this sum equals `(s_A_keptByAlice + s_A_fromAlice + s_A_fromAlice) + (s_B_fromBob + s_B_keptByBob + s_B_fromBob) + (s_C_fromCharlie + s_C_fromCharlie + s_C_keptByCharlie)`.
    *   Since the shares for each patient sum to their original secret (e.g., `s_A_keptByAlice + s_A_fromBob + s_A_fromCharlie = S_A`), the final sum is `S_A + S_B + S_C`.
    *   This way, the hospital obtains the total sum of all patient secrets without ever seeing any individual patient's original secret directly.

**Note:** The patient shares (`aliceShares`, `bobShares`, `charlieShares`, and the `collectedShares` lists) are stored as static lists in [`PatientsController.cs`](Controllers/PatientsController.cs). This is suitable for demonstration but means the state is shared across all API calls and would need a more robust storage mechanism for concurrent users or production scenarios.

## API Endpoints

### Patient Endpoints

*   **`GET /api/patients/hello`**
    *   **Description:** A simple endpoint to check if the patient API is responsive.
    *   **Response:** `string` - "Hello from Patient!"

*   **`POST /api/patients/submit-share`**
    *   **Description:** Allows a patient (Alice, Bob, or Charlie) to submit their integer data. The data is then split into three shares, and these shares are distributed among the `aliceCollectedShares`, `bobCollectedShares`, and `charlieCollectedShares` lists.
    *   **Query Parameter:** `patientName` (string, e.g., "alice", "bob", "charlie")
    *   **Request Body:** `integer` (e.g., `100`). Send as raw JSON.
    *   **Example URL:** `POST http://localhost:<port>/api/patients/submit-share?patientName=alice`
    *   **Example Body:** `10`
    *   **Headers:** `Content-Type: application/json`
    *   **Response:** JSON object detailing the submission and the current state of all collected shares.

### Hospital Endpoints

These endpoints require an `ApiKey` header for authorization. The default API key is hardcoded as `"YourHospitalApiKey"` in [`Services/HospitalService.cs`](Services/HospitalService.cs).

*   **`GET /api/patients/see-shares`**
    *   **Description:** Allows the hospital to view the raw shares originally submitted by each patient and the shares currently held in each of the `collectedShares` lists.
    *   **Headers:** `ApiKey: YourHospitalApiKey`
    *   **Response:** JSON object containing `aliceShares`, `bobShares`, `charlieShares`, `aliceCollectedShares`, `bobCollectedShares`, and `charlieCollectedShares`.

*   **`GET /api/patients/get-aggregate-value`**
    *   **Description:** Allows the hospital to retrieve the securely aggregated sum of all data submitted by the patients. This endpoint combines the sums of shares from `aliceCollectedShares`, `bobCollectedShares`, and `charlieCollectedShares`.
    *   **Headers:** `ApiKey: YourHospitalApiKey`
    *   **Response:** `string` - e.g., "Aggregate Value: 300"
    *   **Note:** This endpoint will return a `400 Bad Request` if not all three patients (Alice, Bob, and Charlie) have submitted their shares (i.e., if each `collectedShares` list does not contain exactly 3 shares). After successful aggregation, the share lists are cleared for the next round.

## Testing with Postman

Replace `<port>` with the actual port your application is running on (e.g., 5001 for HTTPS or 5000 for HTTP).

1.  **Submit Alice's Data:**
    *   **Method:** `POST`
    *   **URL:** `http://localhost:<port>/api/patients/submit-share?patientName=alice`
    *   **Headers:** `Content-Type: application/json`
    *   **Body (raw, JSON):** `10`

2.  **Submit Bob's Data:**
    *   **Method:** `POST`
    *   **URL:** `http://localhost:<port>/api/patients/submit-share?patientName=bob`
    *   **Headers:** `Content-Type: application/json`
    *   **Body (raw, JSON):** `20`

3.  **Submit Charlie's Data:**
    *   **Method:** `POST`
    *   **URL:** `http://localhost:<port>/api/patients/submit-share?patientName=charlie`
    *   **Headers:** `Content-Type: application/json`
    *   **Body (raw, JSON):** `30`

4.  **(Hospital) See Shares (Optional):**
    *   **Method:** `GET`
    *   **URL:** `http://localhost:<port>/api/patients/see-shares`
    *   **Headers:** `ApiKey: YourHospitalApiKey`

5.  **(Hospital) Get Aggregate Value:**
    *   **Method:** `GET`
    *   **URL:** `http://localhost:<port>/api/patients/get-aggregate-value`
    *   **Headers:** `ApiKey: YourHospitalApiKey`
    *   **Expected Response (for data 10, 20, 30):** `"Aggregate Value: 60"`

## Security Features Demonstrated

*   **Additive Secret Sharing:** Patient data is split into shares by [`SecretSharingService.Split()`](Services/SecretSharingService.cs). The way shares are distributed means that no single collected share list reveals an original secret directly. Aggregation is performed on these shares.
*   **API Key Authorization:** Hospital-specific endpoints (`/see-shares`, `/get-aggregate-value`) are protected. Access requires a valid API key sent in the `ApiKey` header. This is implemented using:
    *   [`HospitalApiKeyAuthorizationAttribute`](Authorization/HospitalApiKeyAuthorizationAttribute.cs)
    *   [`HospitalApiKeyAuthorizationHandler`](Authorization/HospitalApiKeyAuthorizationHandler.cs)
    *   [`HospitalApiKeyAuthorizationRequirement`](Authorization/HospitalApiKeyAuthorizationRequirement.cs)
    *   The API key is validated by [`HospitalService`](Services/HospitalService.cs).
*   **Input Validation:** The `POST /api/patients/submit-share` endpoint in [`PatientsController.cs`](Controllers/PatientsController.cs) checks if the submitted `data` is non-negative.

## Project Structure

*   **`Controllers/`**: Contains [`PatientsController.cs`](Controllers/PatientsController.cs) which handles API requests.
*   **`Services/`**:
    *   [`SecretSharingService.cs`](Services/SecretSharingService.cs): Implements the logic for splitting secrets into shares.
    *   [`HospitalService.cs`](Services/HospitalService.cs): Manages hospital-related logic, including API key validation.
*   **`Authorization/`**: Contains classes for custom API key authorization:
    *   [`HospitalApiKeyAuthorizationAttribute.cs`](Authorization/HospitalApiKeyAuthorizationAttribute.cs)
    *   [`HospitalApiKeyAuthorizationHandler.cs`](Authorization/HospitalApiKeyAuthorizationHandler.cs)
    *   [`HospitalApiKeyAuthorizationRequirement.cs`](Authorization/HospitalApiKeyAuthorizationRequirement.cs)
*   **`Startup.cs`**: Configures services, dependency injection, and the application's request pipeline, including authorization policies.
*   **`Program.cs`**: The main entry point for the application.
*   **`appsettings.json` / `appsettings.Development.json`**: Configuration files.
