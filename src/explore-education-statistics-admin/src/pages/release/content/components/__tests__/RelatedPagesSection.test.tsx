import RelatedPagesSection from '@admin/pages/release/content/components/RelatedPagesSection';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import _releaseContentRelatedInformationService from '@admin/services/releaseContentRelatedInformationService';
import { generateEditableRelease } from '@admin-test/generators/releaseContentGenerators';
import { BasicLink } from '@common/services/publicationService';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import render from '@common-test/render';

jest.mock('@admin/services/releaseContentRelatedInformationService');
const releaseContentRelatedInformationService =
  _releaseContentRelatedInformationService as jest.Mocked<
    typeof _releaseContentRelatedInformationService
  >;

describe('RelatedPagesSection', () => {
  const testRelease = generateEditableRelease({});
  const testReleaseWithNoRelatedPages = generateEditableRelease({
    relatedInformation: [],
  });

  describe('editing mode', () => {
    test('renders correctly with related pages', () => {
      render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );
      expect(
        screen.getByRole('heading', { name: 'Related pages' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Add related page link' }),
      ).toBeInTheDocument();
      const pages = screen.getAllByRole('listitem');
      expect(pages).toHaveLength(1);
      expect(
        within(pages[0]).getByRole('link', {
          name: 'Related information description',
        }),
      ).toHaveAttribute('href', 'https://test.com');
      expect(
        within(pages[0]).getByRole('button', { name: 'Remove link' }),
      ).toBeInTheDocument();
    });

    test('renders correctly without related pages', () => {
      render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testReleaseWithNoRelatedPages} />
        </EditingContextProvider>,
      );
      expect(
        screen.getByRole('heading', { name: 'Related pages' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Add related page link' }),
      ).toBeInTheDocument();
      expect(screen.queryByRole('listitem')).not.toBeInTheDocument();
    });

    test('shows the form when click the add button', async () => {
      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add related page link' }),
      );

      expect(screen.getByLabelText('Title')).toBeInTheDocument();
      expect(screen.getByLabelText('Link URL')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Create link' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('shows a validation error when no title is set', async () => {
      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add related page link' }),
      );

      await user.click(screen.getByLabelText('Title'));
      await user.click(screen.getByRole('button', { name: 'Create link' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', {
            name: 'Enter a link title',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows a validation error when no url is set', async () => {
      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add related page link' }),
      );

      await user.click(screen.getByLabelText('Link URL'));
      await user.click(screen.getByRole('button', { name: 'Create link' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', {
            name: 'Enter a link URL',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows a validation error when the url is invalid', async () => {
      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add related page link' }),
      );

      await user.type(screen.getByLabelText('Link URL'), 'Not a url');
      await user.click(screen.getByRole('button', { name: 'Create link' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', {
            name: 'Enter a valid link URL',
          }),
        ).toBeInTheDocument();
      });
    });

    test('successfully adds a link', async () => {
      const newLinks: BasicLink[] = [
        { ...testRelease.relatedInformation[0] },
        { description: 'Test title', id: 'test-id', url: 'https://gov.uk' },
      ];
      releaseContentRelatedInformationService.create.mockResolvedValue(
        newLinks,
      );

      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add related page link' }),
      );

      await user.type(screen.getByLabelText('Title'), 'Test title');
      await user.type(screen.getByLabelText('Link URL'), 'https://gov.uk');
      await user.click(screen.getByRole('button', { name: 'Create link' }));

      expect(
        await screen.findByRole('button', { name: 'Add related page link' }),
      );

      const pages = screen.getAllByRole('listitem');
      expect(pages).toHaveLength(2);
      expect(
        within(pages[0]).getByRole('link', {
          name: 'Related information description',
        }),
      ).toHaveAttribute('href', 'https://test.com');
      expect(
        within(pages[0]).getByRole('button', { name: 'Remove link' }),
      ).toBeInTheDocument();

      expect(
        within(pages[1]).getByRole('link', {
          name: 'Test title',
        }),
      ).toHaveAttribute('href', 'https://gov.uk');
      expect(
        within(pages[1]).getByRole('button', { name: 'Remove link' }),
      ).toBeInTheDocument();
    });

    test('successfully removes a link', async () => {
      releaseContentRelatedInformationService.delete.mockResolvedValue([]);

      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      const pages = screen.getAllByRole('listitem');
      expect(pages).toHaveLength(1);
      expect(
        within(pages[0]).getByRole('link', {
          name: 'Related information description',
        }),
      ).toHaveAttribute('href', 'https://test.com');

      await user.click(
        within(pages[0]).getByRole('button', { name: 'Remove link' }),
      );

      waitFor(() =>
        expect(screen.queryByRole('listitem')).not.toBeInTheDocument(),
      );
    });
  });

  describe('preview mode', () => {
    test('renders correctly with related pages', () => {
      render(
        <EditingContextProvider editingMode="preview">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );
      expect(
        screen.getByRole('heading', { name: 'Related pages' }),
      ).toBeInTheDocument();
      const pages = screen.getAllByRole('listitem');
      expect(pages).toHaveLength(1);
      expect(
        within(pages[0]).getByRole('link', {
          name: 'Related information description',
        }),
      ).toHaveAttribute('href', 'https://test.com');

      expect(
        screen.queryByRole('button', { name: 'Remove link' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Add related page link' }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly without related pages', () => {
      render(
        <EditingContextProvider editingMode="preview">
          <RelatedPagesSection release={testReleaseWithNoRelatedPages} />
        </EditingContextProvider>,
      );
      expect(
        screen.queryByRole('heading', { name: 'Related pages' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Add related page link' }),
      ).not.toBeInTheDocument();
      expect(screen.queryByRole('listitem')).not.toBeInTheDocument();
    });
  });
});
