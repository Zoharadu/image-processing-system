import { Component, OnInit, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { ImageStatus } from '../../../../core/enums/image-status.enum';
import { ImageListDto } from '../../../../core/models/image-list-dto.model';
import { ImageApiService } from '../../../../core/services/image-api.service';

@Component({
  selector: 'app-completed-images',
  imports: [],
  templateUrl: './completed-images.html',
  styleUrl: './completed-images.scss'
})
export class CompletedImages implements OnInit {
  private readonly imageApiService = inject(ImageApiService);

  protected readonly images = signal<ImageListDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadCompletedImages();
  }

  protected getImageDownloadUrl(id: string): string {
    return this.imageApiService.getImageDownloadUrl(id);
  }

  protected getStatusLabel(status: ImageStatus): string {
    return status === ImageStatus.ProcessError ? 'ProcessError' : 'Finished';
  }

  private loadCompletedImages(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.imageApiService
      .getCompletedImages()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (images) => this.images.set(images),
        error: () => {
          this.images.set([]);
          this.errorMessage.set('We could not load completed images right now. Please try again later.');
        }
      });
  }
}
