import InvalidContentDetails from '@admin/components/editable/InvalidContentDetails';
import { InvalidContentError } from '@admin/components/editable/utils/getInvalidContent';
import { render, screen } from '@testing-library/react';

describe('InvalidContentDetails', () => {
  test('renders clickHereLink errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'clickHereLinkText',
      },
      {
        type: 'clickHereLinkText',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(screen.getByRole('button', { name: /2 "click here" links/ }));
  });

  test('renders repeatedLinkText errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'repeatedLinkText',
        message: 'Repeated link text',
        details: 'url-1, url-2',
      },
      {
        type: 'repeatedLinkText',
        message: 'Repeated link text',
        details: 'url-2, url-1',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(
      screen.getByRole('button', {
        name: /2 links have the same text with different URLs/,
      }),
    );
    expect(screen.getByText('Repeated link text: url-1, url-2'));
  });

  test('renders oneWordLinkText errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'oneWordLinkText',
        message: 'word',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(
      screen.getByRole('button', {
        name: /1 link with one word link text/,
      }),
    );
    expect(screen.getByText('word'));
  });

  test('renders urlLinkText errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'urlLinkText',
        message: 'https://explore-education-statistics.service.gov.uk/',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(
      screen.getByRole('button', {
        name: /1 link with a URL as link text/,
      }),
    );
    expect(
      screen.getByText('https://explore-education-statistics.service.gov.uk/'),
    );
  });

  test('renders skippedHeadingLevel errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'skippedHeadingLevel',
        message:
          'h3 (Provider reporting during the COVID-19 pandemic) to h5 (Absence decreased across all school types)',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(
      screen.getByRole('button', {
        name: /1 skipped heading level/,
      }),
    );
    expect(
      screen.getByText(
        'h3 (Provider reporting during the COVID-19 pandemic) to h5 (Absence decreased across all school types)',
      ),
    );
  });

  test('renders boldAsHeading errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'boldAsHeading',
        message:
          'Overall absence rates decreased compared to the previous Autumn term',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(
      screen.getByRole('button', {
        name: /1 line with bold text used instead of a heading/,
      }),
    );
    expect(
      screen.getByText(
        'Overall absence rates decreased compared to the previous Autumn term',
      ),
    );
  });

  test('renders emptyHeading errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'emptyHeading',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(
      screen.getByRole('button', {
        name: /1 empty heading/,
      }),
    );
  });

  test('renders missingTableHeaders errors', () => {
    const testErrors: InvalidContentError[] = [
      {
        type: 'missingTableHeaders',
      },
    ];
    render(<InvalidContentDetails errors={testErrors} />);

    expect(
      screen.getByRole('button', {
        name: /1 table has missing headers/,
      }),
    );
  });
});
