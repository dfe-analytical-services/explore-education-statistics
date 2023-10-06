import React from 'react';
import { render, screen } from '@testing-library/react';
import * as ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import MethodologyHelpAndSupportSection from '../MethodologyHelpAndSupportSection';
import mockPublicationSummary from './__data__/mock-data';

jest.mock('@common/modules/find-statistics/components/ContactUsSection');

describe('Methodology Help and Support Section', () => {
  beforeEach(() => {
    jest
      .spyOn(ContactUsSection, 'default')
      .mockReturnValue(<div>This is a mocked ContactUsSection</div>);

    render(
      <MethodologyHelpAndSupportSection
        owningPublication={mockPublicationSummary}
      />,
    );
  });

  it('Renders', () => {
    expect(
      screen.getByRole('heading', { name: /Help and support/ }),
    ).toBeVisible();
  });

  it('Renders a Contact Us Section', () => {
    expect(screen.getByText('This is a mocked ContactUsSection')).toBeVisible();
  });
});
