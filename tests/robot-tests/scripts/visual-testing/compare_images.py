# Thanks to https://pyimagesearch.com/2017/06/19/image-difference-with-opencv-and-python for the sample code for image
# comparison.
# Thanks to https://stackoverflow.com/questions/44720580/resize-image-canvas-to-maintain-square-aspect-ratio-in-python-opencv
# for the sample code for image resizing.
import os

import cv2
import imutils
import numpy as np
from skimage.metrics import structural_similarity as compare_ssim


def resize_and_pad(img, size, padColor=0):
    h, w = img.shape[:2]
    sh, sw = size

    # interpolation method
    if h > sh or w > sw:  # shrinking image
        interp = cv2.INTER_AREA
    else:  # stretching image
        interp = cv2.INTER_CUBIC

    # aspect ratio of image
    aspect = w / h

    # compute scaling and pad sizing
    if aspect > 1:  # horizontal image
        new_w = sw
        new_h = np.round(new_w / aspect).astype(int)
        pad_vert = (sh - new_h) / 2
        pad_top, pad_bot = np.floor(pad_vert).astype(int), np.ceil(pad_vert).astype(int)
        pad_left, pad_right = 0, 0
    elif aspect < 1:  # vertical image
        new_h = sh
        new_w = np.round(new_h * aspect).astype(int)
        pad_horz = (sw - new_w) / 2
        pad_left, pad_right = np.floor(pad_horz).astype(int), np.ceil(pad_horz).astype(int)
        pad_top, pad_bot = 0, 0
    else:  # square image
        new_h, new_w = sh, sw
        pad_left, pad_right, pad_top, pad_bot = 0, 0, 0, 0

    # set pad color
    if len(img.shape) == 3 and not isinstance(
        padColor, (list, tuple, np.ndarray)
    ):  # color image but only one color provided
        padColor = [padColor] * 3

    print(f"Resizing: {pad_top}, {pad_bot}, {pad_left}, {pad_right}")

    # scale and pad
    scaled_img = cv2.resize(img, (new_w, new_h), interpolation=interp)
    scaled_img = cv2.copyMakeBorder(
        scaled_img, pad_top, pad_bot, pad_left, pad_right, borderType=cv2.BORDER_CONSTANT, value=padColor
    )

    return scaled_img


def compare_images(
    first_filepath, second_filepath, diff_folder, original_filename, diff_threshold=0, visual_diff=False
):

    try:
        # load the two input images
        image1 = cv2.imread(first_filepath)
        image2 = cv2.imread(second_filepath)

        image1h, image1w = image1.shape[:2]
        image2h, image2w = image2.shape[:2]

        # resize the image canvases to be the same size before comparison
        if image1w != image2w or image1h != image2h:
            image1_resized = resize_and_pad(image1, (max(image1h, image2h), max(image1w, image2w)))
            image2_resized = resize_and_pad(image2, (max(image1h, image2h), max(image1w, image2w)))
        else:
            image1_resized = image1
            image2_resized = image2

        # convert the images to grayscale
        image1 = cv2.cvtColor(image1_resized, cv2.COLOR_BGR2GRAY)
        image2 = cv2.cvtColor(image2_resized, cv2.COLOR_BGR2GRAY)

        # compute the Structural Similarity Index (SSIM) between the two
        # images, ensuring that the difference image is returned
        (score, diff_image) = compare_ssim(image1, image2, full=True)
        diff_image = (diff_image * 255).astype("uint8")

        if score < (1 - diff_threshold):
            print(
                f"Visual difference detected in snapshot {os.path.basename(first_filepath)} - similarity : {format(score)}"
            )

            # threshold the difference image, followed by finding contours to
            # obtain the regions of the two input images that differ
            thresh = cv2.threshold(diff_image, 0, 255, cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
            contours = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
            contours = imutils.grab_contours(contours)

            # loop over the contours
            for contour in contours:
                # compute the bounding box of the contour and then draw the
                # bounding box on both input images to represent where the two
                # images differ
                (x, y, w, h) = cv2.boundingRect(contour)
                cv2.rectangle(image1_resized, (x, y), (x + w, y + h), (0, 0, 255), 2)
                cv2.rectangle(image2_resized, (x, y), (x + w, y + h), (0, 0, 255), 2)

                os.makedirs(diff_folder, exist_ok=True)
                cv2.imwrite(f"{diff_folder}/first-{original_filename}", image1)
                cv2.imwrite(f"{diff_folder}/second-{original_filename}", image2)
                cv2.imwrite(f"{diff_folder}/diff-{original_filename}", image2_resized)

                if visual_diff:
                    # show the output images
                    cv2.imshow("Original", image1)
                    cv2.imshow("Modified", image2)
                    cv2.imshow("Diff", diff_image)
                    cv2.imshow("Thresh", thresh)
                    cv2.waitKey(0)
                    cv2.destroyAllWindows()

    except BaseException:
        print(f"Could not compare image {first_filepath}")
