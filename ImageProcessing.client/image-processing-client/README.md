# Image Processing

Image Processing is a full-stack application for uploading images, running them through backend processing pipelines, and viewing completed images with their processing history. The Angular client provides upload, completed image list, image details, and active pipeline dashboard pages. The .NET backend stores uploaded images, processes them in the background, and exposes the API used by the client.

## Backend Startup

From the backend folder:

```bash
cd ../../ImageProcessing.Api
dotnet run --project ImageProcessing.Api/ImageProcessing.Api.csproj --launch-profile https
```

The HTTPS launch profile serves the API at `https://localhost:7194`, which matches the frontend `apiBaseUrl`.

## Frontend Startup

From this frontend folder:

```bash
npm install
npm start
```

Open `http://localhost:4200/` after the Angular development server starts.

## Available Routes

- `/` - upload an image.
- `/images` - view completed or failed images.
- `/images/:id` - view image details and pipeline history.
- `/pipelines` - view active pipeline counts and pie chart.

## Technologies Used

- Angular 21
- TypeScript
- RxJS
- Chart.js with ng2-charts
- ASP.NET Core 8
- C#
- ImageSharp
- xUnit
