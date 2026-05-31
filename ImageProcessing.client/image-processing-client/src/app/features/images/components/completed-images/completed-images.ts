import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, interval } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ImageStatus } from '../../../../core/enums/image-status.enum';
import { ImageListDto } from '../../../../core/models/image-list-dto.model';
import { ImageApiService } from '../../../../core/services/image-api.service';

@Component({
  selector: 'app-completed-images',
  imports: [RouterLink],
  templateUrl: './completed-images.html',
  styleUrl: './completed-images.scss'
})
export class CompletedImages implements OnInit {
  private readonly imageApiService = inject(ImageApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly images = signal<ImageListDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadCompletedImages();
    interval(20000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadCompletedImages(false));
  }

  protected getImageDownloadUrl(id: string): string {
    return this.imageApiService.getImageDownloadUrl(id);
  }

  protected getStatusLabel(status: ImageStatus): string {
    return status === ImageStatus.ProcessError ? 'ProcessError' : 'Finished';
  }

  private loadCompletedImages(showLoading = true): void {
    if (showLoading) {
      this.isLoading.set(true);
    }
    this.errorMessage.set(null);

    this.imageApiService
      .getCompletedImages()
      .pipe(
        finalize(() => {
          if (showLoading) {
            this.isLoading.set(false);
          }
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (images) => this.images.set(images),
        error: () => {
          this.images.set([]);
          this.errorMessage.set('We could not load completed images right now. Please try again later.');
        }
      });
  }
}
