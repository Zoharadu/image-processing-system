import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ImageStatus } from '../../../../core/enums/image-status.enum';
import { PipelineType } from '../../../../core/enums/pipeline-type.enum';
import { ImageDetailsDto } from '../../../../core/models/image-details-dto.model';
import { ImageApiService } from '../../../../core/services/image-api.service';

@Component({
  selector: 'app-image-details',
  imports: [RouterLink],
  templateUrl: './image-details.html',
  styleUrl: './image-details.scss'
})
export class ImageDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly imageApiService = inject(ImageApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly image = signal<ImageDetailsDto | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const imageId = params.get('id');

      if (imageId === null) {
        this.image.set(null);
        this.isLoading.set(false);
        this.errorMessage.set('We could not find this image. Please return to completed images and try again.');
        return;
      }

      this.loadImageDetails(imageId);
    });
  }

  protected getImageDownloadUrl(id: string): string {
    return this.imageApiService.getImageDownloadUrl(id);
  }

  protected getStatusLabel(status: ImageStatus): string {
    switch (status) {
      case ImageStatus.InProcess:
        return 'In process';
      case ImageStatus.ProcessError:
        return 'Process error';
      case ImageStatus.Finished:
      default:
        return 'Finished';
    }
  }

  protected getPipelineName(pipeline: PipelineType): string {
    switch (pipeline) {
      case PipelineType.ImagePipeline:
        return 'ImagePipeline';
      case PipelineType.SquarePipeline:
        return 'SquarePipeline';
      case PipelineType.CirclePipeline:
        return 'CirclePipeline';
      case PipelineType.SlowPipeline:
        return 'SlowPipeline';
      case PipelineType.StarPipeline:
        return 'StarPipeline';
      default:
        return 'Unknown pipeline';
    }
  }

  private loadImageDetails(imageId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.image.set(null);

    this.imageApiService
      .getImageDetails(imageId)
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (image) => this.image.set(image),
        error: () => {
          this.errorMessage.set('We could not load this image right now. Please try again later.');
        }
      });
  }
}
