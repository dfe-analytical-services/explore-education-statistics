import last from 'lodash/last';

export type ParsedContentDisposition =
  | {
      type: 'inline';
    }
  | {
      type: 'attachment';
      filename?: string;
    };

/**
 * Parses the details out of a `Content-Disposition` {@param header}.
 *
 * Note that this currently has been written to handle the most
 * basic use-cases (e.g. using just the `filename` parameter) and has
 * NOT been written to parse every possible option available.
 *
 * For a reference implementation, see the following:
 * https://github.com/jshttp/content-disposition
 * It should be noted that this library only works in a
 * NodeJS environment so is not suitable for our needs.
 */
export default function parseContentDisposition(
  header: string,
): ParsedContentDisposition {
  if (!header) {
    throw new Error('Cannot parse empty header');
  }

  const params = header.split(';');
  const type = params[0].trim();

  if (type !== 'inline' && type !== 'attachment') {
    throw new Error('Must specify type in first parameter');
  }

  if (type === 'inline') {
    return {
      type,
    };
  }

  if (!params[1]) {
    throw new Error('Must specify filename in second parameter');
  }

  const [filenameKey, filenameValue] = params[1].trim().split('=');

  if (filenameKey !== 'filename') {
    throw new Error('Must specify filename in second parameter');
  }

  if (typeof filenameValue === 'undefined') {
    throw new Error('Invalid filename parameter');
  }

  let filename = filenameValue.trim();

  if (!filename || filename === '""') {
    throw new Error('Filename parameter cannot be empty');
  }

  if (filename[0] !== '"' || last(filename) !== '"') {
    throw new Error('Cannot parse unquoted filename parameter');
  }

  // Remove quotes and replace escaped quotes
  // eslint-disable-next-line no-control-regex
  filename = filename.slice(1, -1).replace(/\\([\u0000-\u007f])/g, '$1');

  return {
    type,
    filename,
  };
}
