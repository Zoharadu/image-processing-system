import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ImageDetailsDto } from '../models/image-details-dto.model';
import { ImageListDto } from '../models/image-list-dto.model';
import { PipelineStatusDto } from '../models/pipeline-status-dto.model';

@Injectable({
  providedIn: 'root'
})

export class ImageApiService {
  private readonly apiBaseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  uploadImage(file: File): Observable<{ id: string }> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ id: string }>(`${this.apiBaseUrl}/images`, formData);
  }

  getCompletedImages(): Observable<ImageListDto[]> {
    return this.http.get<ImageListDto[]>(`${this.apiBaseUrl}/images`);
  }

  getImageDetails(id: string): Observable<ImageDetailsDto> {
    return this.http.get<ImageDetailsDto>(`${this.apiBaseUrl}/images/${id}`);
  }

  getActivePipelines(): Observable<PipelineStatusDto[]> {
    return this.http.get<PipelineStatusDto[]>(`${this.apiBaseUrl}/pipelines`);
  }

  getImageDownloadUrl(id: string): string {
    return `${this.apiBaseUrl}/images/${id}/download`;
  }
}
