using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Chat;
using System.Xml.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = "D:\\Desktop\\students.txt";
        if (!File.Exists(filePath))
        {
            Console.WriteLine("students.txt not found.");
            return;
        }

        var studentLines = File.ReadAllLines(filePath);
        string studentData = string.Join("\n", studentLines);

        string prompt = $@"
You are a classroom organization assistant.

Here is a list of students with name, age, and interests:

{studentData}

Please create the best seating order as a list of names based on the gender and their interests.
The classroom has 3 desks per row and 5 rows (15 desks total).
Each desk has 2 seats (30 students max).

Return ONLY the ordered list of names, one per line, in seating order: left to right, top to bottom, 2 per desk.
Do not return anything else except names.
";

        var api = new OpenAIAPI(""); // Replace with your real API key

        var result = await api.Chat.CreateChatCompletionAsync(new ChatRequest
        {
            Model = "gpt-4",
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatMessageRole.User, prompt)
            }
        });

        var responseText = result.Choices[0].Message.Content.Trim();

        var students = new List<string>();
        foreach (var line in responseText.Split(new[] { '\n', ',', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            students.Add(line.Trim());
        }

        //  Save as HTML
        string html = GenerateHtmlLayout(students);
        string htmlPath = "seating.html";
        File.WriteAllText(htmlPath, html);
        Console.WriteLine($"HTML seating chart saved to: {htmlPath}");

        // Optionally open the file in browser
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = htmlPath,
            UseShellExecute = true
        });
    }

    static string GenerateHtmlLayout(List<string> students)
    {
        int index = 0;
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Classroom Seating Plan</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; background: #f8f9fa; }
        .row { display: flex; margin-bottom: 15px; }
        .desk {
            background: #ffffff;
            border: 2px solid #007BFF;
            border-radius: 10px;
            width: 180px;
            height: 80px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            margin-right: 20px;
            box-shadow: 2px 2px 5px rgba(0,0,0,0.1);
        }
        .student { font-size: 16px; color: #333; }
    </style>
</head>
<body>
    <h1>🪑 Classroom Seating Chart</h1>
";

        for (int row = 0; row < 5; row++)
        {
            html += "<div class='row'>";

            for (int col = 0; col < 3; col++)
            {
                string s1 = index < students.Count ? students[index++] : "";
                string s2 = index < students.Count ? students[index++] : "";

                html += "<div class='desk'>";
                html += $"<div class='student'>{s1}</div>";
                html += $"<div class='student'>{s2}</div>";
                html += "</div>";
            }

            html += "</div>";
        }

        html += "</body></html>";
        return html;
    }
}
