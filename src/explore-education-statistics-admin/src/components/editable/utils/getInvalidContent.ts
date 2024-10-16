import { JsonElement, JsonElementChild } from '@admin/types/ckeditor';
import parseNumber from '@common/utils/number/parseNumber';

export interface InvalidContentError {
  type:
    | 'repeatedLinkText'
    | 'badLinkText'
    | 'urlLinkText'
    | 'skippedHeadingLevel'
    | 'missingTableHeaders'
    | 'boldAsHeading'
    | 'emptyHeading';
  message?: string;
  details?: string;
}

export default function getInvalidContent(
  elements: JsonElement[],
): InvalidContentError[] {
  const errors: InvalidContentError[] = [];

  elements.forEach(element => {
    if (element.name === 'paragraph') {
      if (
        element.children?.length === 1 &&
        element.children?.[0].attributes?.bold
      ) {
        errors.push({
          type: 'boldAsHeading',
          message: element.children[0].data,
        });
      }

      return;
    }

    if (
      element.name === 'table' &&
      !element.attributes?.headingRows &&
      !element.attributes?.headingColumns
    ) {
      errors.push({
        type: 'missingTableHeaders',
      });
    }
  });

  const allHeadings = elements.filter(element => isHeading(element));

  allHeadings.forEach((heading, index) => {
    if (!heading.children) {
      errors.push({
        type: 'emptyHeading',
      });
      return;
    }

    const level = parseNumber(heading.name.split('heading')[1]);

    if (index === 0) {
      return;
    }

    const previousLevel = parseNumber(
      allHeadings[index - 1].name.split('heading')[1],
    );

    const headingText = heading.children.map(child => child.data).join('');

    if (level && previousLevel && level > previousLevel + 1) {
      const previousHeadingText = allHeadings[index - 1].children
        ?.map(child => child.data)
        .join('');

      errors.push({
        type: 'skippedHeadingLevel',
        message: `h${previousLevel} (${previousHeadingText}) to h${level} (${headingText})`,
      });
    }
  });

  const allLinks = elements
    .flatMap(element => element.children?.flatMap(child => child))
    .reduce<JsonElementChild[]>((acc, el) => {
      if (el?.attributes?.linkHref) {
        acc.push(el);
      }
      return acc;
    }, []);

  allLinks.forEach(link => {
    if (link.data === link.attributes?.linkHref) {
      errors.push({
        type: 'urlLinkText',
        message: link.data,
      });
      return;
    }

    const badLinkTextWords = [
      'click',
      'csv',
      'continue',
      'dashboard',
      'document',
      'download',
      'file',
      'form',
      'guidance',
      'here',
      'info',
      'information',
      'jpeg',
      'jpg',
      'learn',
      'link',
      'more',
      'next',
      'page',
      'pdf',
      'previous',
      'read',
      'site',
      'svg',
      'this',
      'web',
      'webpage',
      'website',
      'word',
      'xslx',
      'click here',
      'click this link',
      'download csv',
      'download document',
      'download file',
      'download here',
      'download jpg',
      'download jpeg',
      'download pdf',
      'download png',
      'download svg',
      'download word',
      'download xslx',
      'further information',
      'go here',
      'learn more',
      'link to',
      'read more',
      'this page',
      'visit this',
      'web page',
      'web site',
    ];

    if (
      link.data &&
      badLinkTextWords.includes(link.data.trim().toLowerCase())
    ) {
      errors.push({
        type: 'badLinkText',
        message: link.data,
      });
      return;
    }

    const repeatedLinks = allLinks.filter(
      l =>
        l.data === link.data &&
        l.attributes?.linkHref !== link.attributes?.linkHref,
    );

    if (repeatedLinks.length) {
      const matchingUrls = repeatedLinks
        .map(l => l.attributes?.linkHref)
        .toString();

      errors.push({
        type: 'repeatedLinkText',
        message: link.data,
        details: `${link.attributes?.linkHref}, ${matchingUrls}`,
      });
    }
  });

  return errors;
}

function isHeading(element: JsonElement) {
  return (
    element.name === 'heading3' ||
    element.name === 'heading4' ||
    element.name === 'heading5'
  );
}
