import { render, screen, within } from '@testing-library/react';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import React from 'react';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';

describe('ReleaseTypesModal', () => {
  test('renders with default trigger button', () => {
    render(<ReleaseTypesModal />);

    expect(
      screen.getByRole('button', { name: 'What are release types?' }),
    ).toBeInTheDocument();
  });

  test('renders a no-JS fallback with the default trigger button', () => {
    render(<ReleaseTypesModal />);

    const fallback = screen.getByTestId('modal-no-js-fallback');
    expect(
      within(fallback).getByText('What are release types?'),
    ).toBeInTheDocument();
    expect(
      within(fallback).getByText('Accredited official statistics'),
    ).toBeInTheDocument();
  });

  test('accepts a custom trigger button override', () => {
    render(
      <ReleaseTypesModal
        triggerButton={
          <ButtonText>
            <InfoIcon description="This is Test Info Icon Description Text" />
          </ButtonText>
        }
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'This is Test Info Icon Description Text',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('What are release types?'),
    ).not.toBeInTheDocument();

    // The fallback's summary mirrors the default trigger's label, so it is
    // only offered with the default trigger.
    expect(
      screen.queryByTestId('modal-no-js-fallback'),
    ).not.toBeInTheDocument();
  });
});
