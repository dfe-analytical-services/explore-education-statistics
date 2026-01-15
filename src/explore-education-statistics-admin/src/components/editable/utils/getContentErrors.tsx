import React, { ReactNode } from 'react';
import getInvalidImages from '@admin/components/editable/utils/getInvalidImages';
import { Element, JsonElement } from '@admin/types/ckeditor';
import getInvalidLinks, {
  InvalidUrl,
} from '@admin/components/editable/utils/getInvalidLinks';
import getInvalidContent from '@admin/components/editable/utils/getInvalidContent';
import WarningMessage from '@common/components/WarningMessage';
import InvalidContentDetails from '@admin/components/editable/InvalidContentDetails';

export default function getContentErrors(
  elements: Element[],
  {
    checkLinks,
    checkImages,
    checkContent,
  }: {
    checkLinks?: boolean;
    checkImages?: boolean;
    checkContent?: boolean;
  } = {
    checkLinks: true,
    checkImages: true,
    checkContent: true,
  },
): { errorMessage: string; contentErrorDetails: ReactNode } | undefined {
  // Convert to json to make it easier to process and test.
  // Have to convert from Record<string | unknown> to unknown then to our
  // JsonElement type to be able to access object properties
  const elementsJson = elements.map(element => element.toJSON() as unknown);

  const invalidImages = checkImages
    ? getInvalidImages(elementsJson as JsonElement[])
    : [];
  const invalidLinks = checkLinks
    ? getInvalidLinks(elementsJson as JsonElement[])
    : [];
  const invalidContent = checkContent
    ? getInvalidContent(elementsJson as JsonElement[])
    : [];

  if (invalidImages.length || invalidLinks.length || invalidContent.length) {
    const invalidImagesMessage =
      invalidImages.length === 1
        ? '1 image does not have alternative text.'
        : `${invalidImages.length} images do not have alternative text.`;

    const invalidLinksMessage =
      invalidLinks.length === 1
        ? '1 link has an invalid URL.'
        : `${invalidLinks.length} links have invalid URLs.`;

    const invalidContentMessage =
      invalidContent.length === 1
        ? '1 accessibility error.'
        : `${invalidContent.length} accessibility errors.`;

    const errorMessage = `Content errors have been found: ${
      invalidImages.length ? invalidImagesMessage : ''
    }  ${invalidLinks.length ? invalidLinksMessage : ''} ${
      invalidContent.length ? invalidContentMessage : ''
    }`;
    const contentErrorDetails = (
      <>
        <WarningMessage className="govuk-!-margin-bottom-1">
          The following problems must be resolved before saving:
        </WarningMessage>
        {!!invalidImages.length && (
          <InvalidImagesDetails errors={invalidImages} />
        )}
        {!!invalidLinks.length && <InvalidLinksDetails errors={invalidLinks} />}
        {!!invalidContent.length && (
          <InvalidContentDetails errors={invalidContent} />
        )}
      </>
    );
    return { errorMessage, contentErrorDetails };
  }
  return undefined;
}
function InvalidImagesDetails({ errors }: { errors: JsonElement[] }) {
  return (
    <>
      <p>
        {errors.length === 1
          ? '1 image does not have alternative text.'
          : `${errors.length} images do not have alternative text.`}
      </p>
      <ul>
        <li>
          Alternative text must be added for all images, for guidance see{' '}
          <a
            href="https://www.w3.org/WAI/tutorials/images/tips/"
            rel="noopener noreferrer nofollow"
            target="_blank"
          >
            W3C tips on writing alternative text (opens in new tab)
          </a>
          .
        </li>
        <li>Images without alternative text are outlined in red.</li>
      </ul>
    </>
  );
}

function InvalidLinksDetails({ errors }: { errors: InvalidUrl[] }) {
  return (
    <>
      <p>The following links have invalid URLs:</p>
      <ul>
        {errors.map(error => (
          <li key={error?.text}>
            {error?.text} ({error?.url})
          </li>
        ))}
      </ul>
    </>
  );
}
