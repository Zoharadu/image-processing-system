import { Routes } from '@angular/router';

import { CompletedImages } from './features/images/components/completed-images/completed-images';
import { ImageUpload } from './features/images/components/image-upload/image-upload';

export const routes: Routes = [
  {
    path: '',
    component: ImageUpload
  },
  {
    path: 'images',
    component: CompletedImages
  }
];
