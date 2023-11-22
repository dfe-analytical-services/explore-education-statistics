import { render, screen } from '@testing-library/react';
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
  });
});
