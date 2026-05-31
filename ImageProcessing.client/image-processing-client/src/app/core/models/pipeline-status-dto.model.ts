import { PipelineType } from '../enums/pipeline-type.enum';

export interface PipelineStatusDto {
  pipelineType: PipelineType;
  activeImagesCount: number;
}
