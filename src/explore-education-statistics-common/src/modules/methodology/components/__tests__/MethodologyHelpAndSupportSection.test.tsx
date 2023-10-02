import React from 'react';
import { render, screen } from '@testing-library/react';
import * as ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import mockContact from '@common/modules/find-statistics/components/__tests__/__data__/mock-data';
import MethodologyHelpAndSupportSection from '../MethodologyHelpAndSupportSection';
import mockMethodology from './__data__/mock-data';

jest.mock('@common/modules/find-statistics/components/ContactUsSection');

describe('Methodology Help and Support Section', () => {
  beforeEach(() => {
    jest
      .spyOn(ContactUsSection, 'default')
      .mockReturnValue(<div>This is a mocked ContactUsSection</div>);

    render(
      <MethodologyHelpAndSupportSection
        methodology={mockMethodology}
        contact={mockContact}
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
