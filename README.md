## Architecture

The worker service runs continuously and:
1. Polls the Service Bus queue for meeting chunks
2. Processes each chunk using the transcription service
3. Stores the transcript in Cosmos DB
4. Handles errors and retries appropriately


## Configuration

Before running the application, set the configuration values in `appsettings.json`


## Running the application 

Set MeetingSummary.TranscriptionWorker project has the startup project

