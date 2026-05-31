import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChartConfiguration, ChartData } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { finalize, interval } from 'rxjs';

import { PipelineType } from '../../../../core/enums/pipeline-type.enum';
import { PipelineStatusDto } from '../../../../core/models/pipeline-status-dto.model';
import { ImageApiService } from '../../../../core/services/image-api.service';

interface PipelineViewModel extends PipelineStatusDto {
  name: string;
  color: string;
}

@Component({
  selector: 'app-active-pipelines',
  imports: [BaseChartDirective],
  templateUrl: './active-pipelines.html',
  styleUrl: './active-pipelines.scss'
})
export class ActivePipelines implements OnInit {
  private readonly imageApiService = inject(ImageApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly segmentColors = ['#2563eb', '#38bdf8', '#60a5fa', '#0ea5e9', '#93c5fd'];

  protected readonly pipelines = signal<PipelineStatusDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly chartType = 'pie';

  protected readonly pipelineItems = computed<PipelineViewModel[]>(() =>
    this.pipelines()
      .filter((pipeline) => pipeline.activeImagesCount > 0)
      .map((pipeline, index) => ({
        ...pipeline,
        name: this.getPipelineName(pipeline.pipelineType),
        color: this.segmentColors[index % this.segmentColors.length]
      }))
  );

  protected readonly totalActiveImages = computed(() =>
    this.pipelineItems().reduce((total, pipeline) => total + pipeline.activeImagesCount, 0)
  );

  protected readonly chartData = computed<ChartData<'pie', number[], string>>(() => {
    const pipelines = this.pipelineItems();

    return {
      labels: pipelines.map((pipeline) => pipeline.name),
      datasets: [
        {
          data: pipelines.map((pipeline) => pipeline.activeImagesCount),
          backgroundColor: pipelines.map((pipeline) => pipeline.color),
          borderColor: '#ffffff',
          borderWidth: 3,
          hoverOffset: 8
        }
      ]
    };
  });

  protected readonly chartOptions: ChartConfiguration<'pie'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const count = context.parsed;
            const imageLabel = count === 1 ? 'image' : 'images';

            return `${context.label}: ${count} active ${imageLabel}`;
          }
        }
      }
    }
  };

  ngOnInit(): void {
    this.loadActivePipelines();
    interval(20000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadActivePipelines(false));
  }

  protected getActiveImageLabel(count: number): string {
    return count === 1 ? '1 active image' : `${count} active images`;
  }

  private loadActivePipelines(showLoading = true): void {
    if (showLoading) {
      this.isLoading.set(true);
    }
    this.errorMessage.set(null);

    this.imageApiService
      .getActivePipelines()
      .pipe(
        finalize(() => {
          if (showLoading) {
            this.isLoading.set(false);
          }
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (pipelines) => this.pipelines.set(pipelines),
        error: () => {
          this.pipelines.set([]);
          this.errorMessage.set('We could not load active pipelines right now. Please try again later.');
        }
      });
  }

  private getPipelineName(pipelineType: PipelineType): string {
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
      default:
        return 'Unknown pipeline';
    }
  }
}
