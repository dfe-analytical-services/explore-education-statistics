import React from 'react';
import { render, screen } from '@testing-library/react';
import * as ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import MethodologyHelpAndSupportSection from '@common/modules/methodology/components/MethodologyHelpAndSupportSection';
import testPublicationSummary from '@common/modules/methodology/components/__tests__/__data__/test-data';

jest.mock('@common/modules/find-statistics/components/ContactUsSection');

describe('MethodologyHelpAndSupportSection', () => {
  beforeEach(() => {
    jest
      .spyOn(ContactUsSection, 'default')
      .mockReturnValue(<div>This is a mocked ContactUsSection</div>);

    render(
      <MethodologyHelpAndSupportSection
        owningPublication={testPublicationSummary}
      />,
    );
  });

  test('that it renders', () => {
    expect(
      screen.getByRole('heading', { name: /Help and support/ }),
    ).toBeVisible();
  });

  test('that it renders a Contact Us Section', () => {
    expect(screen.getByText('This is a mocked ContactUsSection')).toBeVisible();
  });
});
