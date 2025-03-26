import { render, RenderResult, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import ReleaseLabelEditModal, {
  ReleaseLabelFormValues,
} from '../ReleaseLabelEditModal';

describe('ReleaseLabelEditModal', () => {
  test('modal contains a form with a label input field', async () => {
    const handleSubmit = jest.fn();

    await renderModal(handleSubmit, 'release-slug', 'publication-slug');

    expect(screen.getByRole('textbox', { name: 'Label' })).toBeInTheDocument();
  });

  test('cancel button closes modal', async () => {
    const handleSubmit = jest.fn();

    await renderModal(handleSubmit, 'release-slug', 'publication-slug');

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(screen.queryByText('Cancel')).not.toBeInTheDocument();
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  test.each(['', ' ', '  '])(
    'submit button displays correct confirmation warning with EMPTY label',
    async (newLabel: string) => {
      const handleSubmit = jest.fn();

      const { baseElement } = await renderModal(
        handleSubmit,
        'release-slug-initial',
        'publication-slug',
        { label: 'initial' },
      );

      const labelInput = screen.getByRole('textbox', { name: 'Label' });
      await userEvent.clear(labelInput);
      if (newLabel !== '') {
        await userEvent.type(labelInput, newLabel);
      }

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(
        baseElement.querySelector('.govuk-warning-text'),
      ).toHaveTextContent("Removing this release's label");

      expect(baseElement.querySelector('#before-url')).toHaveValue(
        'http://localhost/find-statistics/publication-slug/release-slug-initial',
      );
      expect(baseElement.querySelector('#after-url')).toHaveValue(
        'http://localhost/find-statistics/publication-slug/release-slug',
      );
    },
  );

  test.each([
    [{ label: 'initial' }, 'release-slug-initial', 'initial'],
    [{ label: ' initial ' }, 'release-slug-initial', 'initial'],
    [{ label: 'initial 2' }, 'release-slug-initial-2', 'initial 2'],
    [{ label: 'initial  2' }, 'release-slug-initial-2', 'initial 2'],
    [{ label: 'INITIAL' }, 'release-slug-initial', 'INITIAL'],
    [
      { label: 'INITIAL initial' },
      'release-slug-initial-initial',
      'INITIAL initial',
    ],
  ])(
    'submit button displays confirmation warning with INITIAL VALUES when UNCHANGED',
    async (
      initialValues: ReleaseLabelFormValues,
      initialReleaseSlug: string,
      expectedFormattedLabel: string,
    ) => {
      const handleSubmit = jest.fn();

      const { baseElement } = await renderModal(
        handleSubmit,
        initialReleaseSlug,
        'publication-slug',
        initialValues,
      );

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(
        baseElement.querySelector('.govuk-warning-text'),
      ).toHaveTextContent(
        `Changing this release's label to ${expectedFormattedLabel}`,
      );

      expect(baseElement.querySelector('#before-url')).toHaveValue(
        `http://localhost/find-statistics/publication-slug/${initialReleaseSlug}`,
      );
      expect(baseElement.querySelector('#after-url')).toHaveValue(
        `http://localhost/find-statistics/publication-slug/${initialReleaseSlug}`,
      );
    },
  );

  test.each([
    ['initial', 'initial', 'release-slug-initial'],
    ['next', 'next', 'release-slug-next'],
    [' next ', 'next', 'release-slug-next'],
    ['next 2', 'next 2', 'release-slug-next-2'],
    ['next  2', 'next 2', 'release-slug-next-2'],
    ['NEXT', 'NEXT', 'release-slug-next'],
    ['NEXT next', 'NEXT next', 'release-slug-next-next'],
  ])(
    'submit button displays confirmation warning with NEW VALUES when CHANGED',
    async (
      newLabel: string,
      expectedFormattedLabel: string,
      expectedNewReleaseSlug: string,
    ) => {
      const handleSubmit = jest.fn();

      const { baseElement } = await renderModal(
        handleSubmit,
        'release-slug-initial',
        'publication-slug',
        { label: 'initial' },
      );

      const labelInput = screen.getByRole('textbox', { name: 'Label' });
      await userEvent.clear(labelInput);
      await userEvent.type(labelInput, newLabel);

      await userEvent.click(screen.getByRole('button', { name: 'Save' }));

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();

      expect(
        baseElement.querySelector('.govuk-warning-text'),
      ).toHaveTextContent(
        `Changing this release's label to ${expectedFormattedLabel}`,
      );

      expect(baseElement.querySelector('#before-url')).toHaveValue(
        'http://localhost/find-statistics/publication-slug/release-slug-initial',
      );
      expect(baseElement.querySelector('#after-url')).toHaveValue(
        `http://localhost/find-statistics/publication-slug/${expectedNewReleaseSlug}`,
      );
    },
  );

  async function renderModal(
    onSubmit: (formValues: ReleaseLabelFormValues) => Promise<void>,
    currentReleaseSlug: string,
    publicationSlug: string,
    initialValues?: ReleaseLabelFormValues,
  ): Promise<RenderResult> {
    const renderResult = render(
      <TestConfigContextProvider>
        <ReleaseLabelEditModal
          currentReleaseSlug={currentReleaseSlug}
          publicationSlug={publicationSlug}
          initialValues={initialValues}
          triggerButton={<button type="button">Open</button>}
          onSubmit={onSubmit}
        />
      </TestConfigContextProvider>,
    );

    expect(
      await screen.findByRole('button', { name: 'Open' }),
    ).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Open' }));

    expect(
      await screen.findByRole('button', { name: 'Save' }),
    ).toBeInTheDocument();

    expect(
      await screen.findByRole('button', { name: 'Cancel' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Edit release label')).toBeInTheDocument();

    return renderResult;
  }
});
