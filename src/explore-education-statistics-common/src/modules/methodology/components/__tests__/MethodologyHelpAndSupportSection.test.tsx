import React from 'react';
import { render, screen } from '@testing-library/react';
import * as ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import MethodologyHelpAndSupportSection from '../MethodologyHelpAndSupportSection';
import {
  testMethodologyPublication,
  testPublicationSummary,
} from './__data__/test-data';

jest.mock('@common/modules/find-statistics/components/ContactUsSection');

describe('Methodology Help and Support Section (PublicationSummary)', () => {
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

describe('Methodology Help and Support Section (mockMethodologyPublication)', () => {
  beforeEach(() => {
    jest
      .spyOn(ContactUsSection, 'default')
      .mockReturnValue(<div>This is a mocked ContactUsSection</div>);

    render(
      <MethodologyHelpAndSupportSection
        owningPublication={testMethodologyPublication}
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
