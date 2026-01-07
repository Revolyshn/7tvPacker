# 7TV Emote to Telegram Sticker Pack Bot

This project contains the source code for a Telegram bot and a companion Mini App that allows users to select emotes from 7tv.app and create a Telegram sticker pack from them.

## Project Structure

The project is divided into two main parts: the backend bot and the frontend Mini App.

### Backend

The backend is a C# .NET 7 application structured as follows:

-   `SevenTvStickers.Host`: The main application that runs the bot.
-   `SevenTvStickers.Core`: Contains the core business logic, including services for handling Telegram updates and creating sticker packs.
-   `SevenTvStickers.Infrastructure`: Contains the implementation for external dependencies, such as the image processing service that uses FFmpeg.

### Frontend (Mini App)

The Mini App is a simple web application built with HTML, CSS, and JavaScript. It fetches trending emotes from the 7TV API, allows users to select their favorites, and sends the selected emote URLs to the bot.

-   `MiniApp/index.html`: The main HTML file.
-   `MiniApp/style.css`: The stylesheet.
-   `MiniApp/app.js`: The JavaScript code that handles the logic.

## How to Run

### Prerequisites

-   .NET 7 SDK or later.
-   FFmpeg installed and available in your system's `PATH`.
-   A Telegram Bot Token obtained from BotFather.

### Backend Setup

1.  **Configure the bot token:** Open the `SevenTvStickers.Host/appsettings.json` file and replace the placeholder `"YOUR_TELEGRAM_BOT_TOKEN_HERE"` with your actual Telegram bot token.

2.  **Build and run the project:** Open a terminal in the root directory of the project and run the following commands. The first command restores the necessary NuGet packages, and the second command builds and runs the main host application.

    ```bash
    dotnet restore
    dotnet run --project SevenTvStickers.Host
    ```

    The bot should now be running and listening for updates from Telegram.

### Frontend Setup

1.  **Deploy the Mini App:** Deploy the contents of the `MiniApp` directory to a web server with HTTPS support (e.g., GitHub Pages, Netlify, Vercel).
2.  **Configure the Mini App in BotFather:**
    -   Open BotFather in Telegram.
    -   Use the `/mybots` command, select your bot, and go to "Bot Settings" -> "Menu Button".
    -   Configure the menu button to launch your Mini App's URL.

## Recommendations

### Optimization

-   **Caching:** Implement caching for downloaded emotes to avoid re-downloading and processing the same emote multiple times.
-   **Queueing:** For large sticker packs, consider using a background job queue (e.g., Hangfire) to process the requests asynchronously.
-   **FFmpeg Optimization:** Fine-tune the FFmpeg commands for better performance and smaller file sizes.

### Security

-   **Input Validation:** Sanitize and validate all input from the user and the 7TV API.
-   **Rate Limiting:** Implement rate limiting to prevent abuse of the bot.
-   **Error Handling:** Improve error handling to provide more specific feedback to the user.
