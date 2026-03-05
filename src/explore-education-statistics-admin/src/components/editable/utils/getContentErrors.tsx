import React, { ReactNode } from 'react';
import InvalidContentDetails from '@admin/components/editable/InvalidContentDetails';
import checkMaxLength from '@admin/components/editable/utils/checkMaxLength';
import getInvalidContent from '@admin/components/editable/utils/getInvalidContent';
import getInvalidImages from '@admin/components/editable/utils/getInvalidImages';
import getInvalidLinks, {
  InvalidUrl,
} from '@admin/components/editable/utils/getInvalidLinks';
import { Element, JsonElement } from '@admin/types/ckeditor';
import WarningMessage from '@common/components/WarningMessage';

interface ContentErrorOptions {
  characterLimit?: number;
  checkLinks?: boolean;
  checkImages?: boolean;
  checkContent?: boolean;
}

interface ContentErrorResult {
  errorMessage: string;
  contentErrorDetails: ReactNode;
}

export default function getContentErrors(
  elements: Element[],
  {
    characterLimit,
    checkLinks = true,
    checkImages = true,
    checkContent = true,
  }: ContentErrorOptions = {},
): ContentErrorResult | undefined {
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
  const invalidLength = characterLimit
    ? checkMaxLength(elementsJson as JsonElement[], characterLimit)
    : '';

  if (
    invalidImages.length ||
    invalidLinks.length ||
    invalidContent.length ||
    invalidLength.length
  ) {
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
    }  ${invalidLength ? 'Too much content.' : ''}`;
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
        {!!invalidLength && <p>{invalidLength}</p>}
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
