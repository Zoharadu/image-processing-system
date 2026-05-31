import { Routes } from '@angular/router';

import { ActivePipelines } from './features/images/components/active-pipelines/active-pipelines';
import { CompletedImages } from './features/images/components/completed-images/completed-images';
import { ImageDetails } from './features/images/components/image-details/image-details';
import { ImageUpload } from './features/images/components/image-upload/image-upload';

export const routes: Routes = [
  {
    path: '',
    component: ImageUpload
  },
  {
    path: 'images',
    component: CompletedImages
  },
  {
    path: 'pipelines',
    component: ActivePipelines
  },
  {
    path: 'images/:id',
    component: ImageDetails
  }
];
