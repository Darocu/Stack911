using Microsoft.Owin;
using Owin;

namespace ApiService.Middleware;

public static class HomePageMiddleware
{
    public static IAppBuilder UseHomePage(this IAppBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals(new PathString("/")))
            {
                context.Response.ContentType = "text/html; charset=utf-8";

                const string html = """
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Cincinnati Unified Safety Toolkit API</title>
                    <style>
                        * {
                            box-sizing: border-box;
                            margin: 0;
                            padding: 0;
                        }
                        body {
                            font-family: 'Segoe UI', system-ui, sans-serif;
                            background-color: #f8fafc;
                            color: #2d3748;
                            line-height: 1.6;
                            padding: 0;
                            margin: 0;
                        }
                        header {
                            background: linear-gradient(135deg, #004085, #002c5f);
                            color: white;
                            padding: .25rem 1rem;
                            text-align: center;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }
                        header h1 {
                            font-size: 2rem;
                            font-weight: 700;
                        }
                        main {
                            max-width: 1000px;
                            margin: 2rem auto;
                            padding: 0 1.5rem;
                        }
                        h2, h3 {
                            margin-top: 1.5rem;
                            margin-bottom: 1rem;
                            color: #004085;
                        }
                        p {
                            margin-bottom: 1rem;
                            font-size: 1.05rem;
                        }
                        ul {
                            margin-left: 1.5rem;
                            margin-bottom: 1.5rem;
                        }
                        li {
                            margin-bottom: 0.5rem;
                        }
                        code {
                            background: #f1f3f5;
                            padding: 0.2em 0.4em;
                            border-radius: 4px;
                            font-family: monospace;
                        }
                        .buttons {
                            display: flex;
                            flex-wrap: wrap;
                            gap: 1rem;
                            justify-content: center;
                            margin: 2rem 0;
                        }
                        a.button {
                            display: inline-block;
                            padding: 1rem 2rem;
                            background-color: #007bff;
                            color: white;
                            text-decoration: none;
                            font-weight: bold;
                            border-radius: 8px;
                            transition: all 0.3s ease;
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                            text-align: center;
                            min-width: 250px;
                        }
                        a.button:hover {
                            background-color: #0056b3;
                            transform: translateY(-2px);
                            box-shadow: 0 6px 10px rgba(0, 0, 0, 0.15);
                        }
                        footer {
                            text-align: center;
                            padding: 2rem 1rem;
                            color: #a0aec0;
                            font-size: 0.9rem;
                            border-top: 1px solid #e2e8f0;
                            margin-top: 3rem;
                        }
                        @media (max-width: 768px) {
                            .buttons {
                                flex-direction: column;
                                align-items: center;
                            }
                            a.button {
                                width: 100%;
                            }
                        }
                    </style>
                </head>
                <body>
                    <header>
                        <h1>Cincinnati Unified Safety Toolkit API</h1>
                    </header>

                    <main>
                        <h2>Welcome</h2>
                        <p>Welcome to the Cincinnati Unified Safety Toolkit API. This API enables access to safety-related data and services, including incident management, position tracking, radio communications, and more.</p>
                        <p>Please use your authorized API key to access the protected endpoints.</p>

                        <div class="buttons">
                            <a href="https://ecc-intranet/" class="button">Explore the ECC Intranet</a>
                            <a href="/swagger" class="button">Explore API Documentation & Test Endpoints</a>
                        </div>

                        <h3>Getting Started</h3>
                        <ul>
                            <li><strong>API Versioning:</strong> Use <code>/api/v1/</code> as the base path for version 1 endpoints.</li>
                            <li><strong>Authentication:</strong> Provide your API key in the <code>X-API-Key</code> header.</li>
                            <li><strong>Support:</strong> Contact the ECC IT Support for access or questions.</li>
                        </ul>
                    </main>

                    <footer>
                        &copy; 2025 Cincinnati Unified Safety Toolkit. All rights reserved.
                    </footer>
                </body>
                </html>
                """;

                await context.Response.WriteAsync(html);
                return;
            }

            await next.Invoke();
        });
    }
}