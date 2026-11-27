import render from '@common-test/render';
import testTenant, { minimalTestTenant } from '@common-test/testTenant';
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

  test('uses the EES acronym in the ad hoc statistics description by default', async () => {
    const { user } = render(<ReleaseTypesModal />);

    await user.click(
      screen.getByRole('button', { name: 'What are release types?' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByText(/not part of DfE's regular annual official statistical/),
    ).toBeInTheDocument();
  });

  test('uses the acronym from the tenant in the ad hoc statistics description', async () => {
    const { user } = render(<ReleaseTypesModal />, { tenant: testTenant });

    await user.click(
      screen.getByRole('button', { name: 'What are release types?' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByText(/not part of TO's regular annual official statistical/),
    ).toBeInTheDocument();
  });

  test('uses the organisation name in the ad hoc statistics description when the tenant has no acronym', async () => {
    const { user } = render(<ReleaseTypesModal />, {
      tenant: minimalTestTenant,
    });

    await user.click(
      screen.getByRole('button', { name: 'What are release types?' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByText(
        /not part of the Test Organisation's regular annual official statistical/,
      ),
    ).toBeInTheDocument();
  });
});
