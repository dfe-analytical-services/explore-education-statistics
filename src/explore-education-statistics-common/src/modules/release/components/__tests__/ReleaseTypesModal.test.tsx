import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import { TenantContextProvider } from '@common/contexts/TenantContext';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import eesTenant from '@common/tenants/eesTenant';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

describe('ReleaseTypesModal', () => {
  test('renders the trigger button', () => {
    render(<ReleaseTypesModal />);

    expect(
      screen.getByRole('button', { name: 'What are release types?' }),
    ).toBeInTheDocument();
  });

  test('server-renders a no-JS fallback', () => {
    // Server HTML, before any effects run (so before the fallback is
    // swapped for the modal).
    const container = document.createElement('div');
    container.innerHTML = renderToStaticMarkup(
      <TenantContextProvider tenant={eesTenant}>
        <ReleaseTypesModal />
      </TenantContextProvider>,
    );

    const fallback = within(container).getByTestId('modal-no-js-fallback');
    expect(fallback).toHaveTextContent('What are release types?');
    expect(fallback).toHaveTextContent('Accredited official statistics');
  });
});
