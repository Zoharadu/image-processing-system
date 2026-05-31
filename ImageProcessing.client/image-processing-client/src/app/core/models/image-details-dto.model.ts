import { ImageStatus } from '../enums/image-status.enum';
import { PipelineType } from '../enums/pipeline-type.enum';

export interface ImageDetailsDto {
  id: string;
  fileName: string;
  width: number;
  height: number;
  status: ImageStatus;
  pipelineHistory: PipelineType[];
}
