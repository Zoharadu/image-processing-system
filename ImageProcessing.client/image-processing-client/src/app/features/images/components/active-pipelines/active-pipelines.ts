import { Component, OnInit, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { PipelineType } from '../../../../core/enums/pipeline-type.enum';
import { PipelineStatusDto } from '../../../../core/models/pipeline-status-dto.model';
import { ImageApiService } from '../../../../core/services/image-api.service';

@Component({
  selector: 'app-active-pipelines',
  imports: [],
  templateUrl: './active-pipelines.html',
  styleUrl: './active-pipelines.scss'
})
export class ActivePipelines implements OnInit {
  private readonly imageApiService = inject(ImageApiService);

  protected readonly pipelines = signal<PipelineStatusDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadActivePipelines();
  }

  protected getPipelineName(pipelineType: PipelineType): string {
    switch (pipelineType) {
      case PipelineType.ImagePipeline:
        return 'Image pipeline';
      case PipelineType.SquarePipeline:
        return 'Square pipeline';
      case PipelineType.CirclePipeline:
        return 'Circle pipeline';
      case PipelineType.SlowPipeline:
        return 'Slow pipeline';
      case PipelineType.StarPipeline:
        return 'Star pipeline';
    }
  }

  private loadActivePipelines(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.imageApiService
      .getActivePipelines()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (pipelines) => this.pipelines.set(pipelines),
        error: () => {
          this.pipelines.set([]);
          this.errorMessage.set('We could not load active pipelines right now. Please try again later.');
        }
      });
  }
}
