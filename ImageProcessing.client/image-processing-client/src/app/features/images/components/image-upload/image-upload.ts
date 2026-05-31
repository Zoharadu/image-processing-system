import { Component, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { ImageApiService } from '../../../../core/services/image-api.service';

@Component({
  selector: 'app-image-upload',
  imports: [],
  templateUrl: './image-upload.html',
  styleUrl: './image-upload.scss'
})

export class ImageUpload {
  private readonly imageApiService = inject(ImageApiService);

  protected readonly selectedFile = signal<File | null>(null);
  protected readonly isUploading = signal(false);
  protected readonly uploadedImageId = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly canUpload = computed(() => this.selectedFile() !== null && !this.isUploading());

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0) ?? null;

    this.selectedFile.set(file);
    this.uploadedImageId.set(null);
    this.errorMessage.set(null);
  }

  protected upload(): void {
    const file = this.selectedFile();

    if (file === null || this.isUploading()) {
      return;
    }

    this.isUploading.set(true);
    this.uploadedImageId.set(null);
    this.errorMessage.set(null);

    this.imageApiService
      .uploadImage(file)
      .pipe(finalize(() => this.isUploading.set(false)))
      .subscribe({
        next: ({ id }) => this.uploadedImageId.set(id),
        error: () => {
          this.errorMessage.set('Upload failed. Please try again with a valid image file.');
        }
      });
  }
}
