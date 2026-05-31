import { ImageStatus } from '../enums/image-status.enum';

export interface ImageListDto {
  id: string;
  fileName: string;
  width: number;
  height: number;
  status: ImageStatus;
}
