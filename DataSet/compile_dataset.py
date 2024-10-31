import os
import struct
import numpy as np
from PIL import Image

def save_images_to_idx(images, output_file):
    """
    Saves a numpy array of images to an IDX format file.

    Parameters:
    - images: numpy array of shape (num_images, rows, cols)
    - output_file: path to the output IDX file
    """
    num_images, rows, cols = images.shape
    with open(output_file, 'wb') as f:
        # Write the magic number for images (2051)
        f.write(struct.pack('>I', 2051))
        # Write the number of images, rows, and columns
        f.write(struct.pack('>III', num_images, rows, cols))
        # Write image data
        images.tofile(f)
    print(f"Saved {num_images} images to {output_file}")

def load_images_from_folder(folder, image_size=(32, 32)):
    """
    Loads all PNG images from a folder, converts them to grayscale,
    resizes them, and returns as a numpy array.

    Parameters:
    - folder: path to the folder containing PNG images
    - image_size: tuple indicating the desired image size (width, height)

    Returns:
    - Numpy array of images with shape (num_images, rows, cols)
    """
    images = []
    for filename in sorted(os.listdir(folder)):
        if filename.lower().endswith('.png'):
            img_path = os.path.join(folder, filename)
            img = Image.open(img_path).convert('L')  # Convert to grayscale
            img = img.resize(image_size, Image.ANTIALIAS)  # Resize image
            img_array = np.array(img, dtype=np.uint8)
            images.append(img_array)
    images = np.array(images)
    return images

def compile(folder, output_file):
    # Load images from the folder
    images = load_images_from_folder(folder)
    if images.size == 0:
        print("No PNG images found in the specified folder.")
        return

    # Save images to IDX format
    save_images_to_idx(images, output_file)

def main():
    compile('Training', 'training.data');
    compile('Test', 'test.data');

if __name__ == '__main__':
    main()
