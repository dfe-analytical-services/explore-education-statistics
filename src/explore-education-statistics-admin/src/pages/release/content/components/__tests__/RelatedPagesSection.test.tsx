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
        screen.getByRole('button', { name: 'Add related page' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Edit pages' }),
      ).toBeInTheDocument();
      const pages = screen.getAllByRole('listitem');
      expect(pages).toHaveLength(1);
      expect(
        within(pages[0]).getByRole('link', {
          name: 'Related information description',
        }),
      ).toHaveAttribute('href', 'https://test.com');
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
        screen.getByRole('button', { name: 'Add related page' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Edit pages' }),
      ).not.toBeInTheDocument();
      expect(screen.queryByRole('listitem')).not.toBeInTheDocument();
    });

    test('shows the form when click the add button', async () => {
      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add related page' }),
      );

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByLabelText('Title')).toBeInTheDocument();
      expect(modal.getByLabelText('Link URL')).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Save' })).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    test('successfully adds a link', async () => {
      const newLinks: BasicLink[] = [
        { ...testRelease.relatedInformation[0] },
        { description: 'Test title', id: 'test-id', url: 'https://gov.uk' },
      ];
      releaseContentRelatedInformationService.update.mockResolvedValue(
        newLinks,
      );

      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add related page' }),
      );

      const modal = within(screen.getByRole('dialog'));
      await user.type(modal.getByLabelText('Title'), 'Test title');
      await user.type(modal.getByLabelText('Link URL'), 'https://gov.uk');
      await user.click(modal.getByRole('button', { name: 'Save' }));

      await waitFor(() =>
        expect(
          screen.queryByText('Add related page link'),
        ).not.toBeInTheDocument(),
      );

      const pages = screen.getAllByRole('listitem');
      expect(pages).toHaveLength(2);
      expect(
        within(pages[0]).getByRole('link', {
          name: 'Related information description',
        }),
      ).toHaveAttribute('href', 'https://test.com');
      expect(
        within(pages[1]).getByRole('link', {
          name: 'Test title',
        }),
      ).toHaveAttribute('href', 'https://gov.uk');
    });

    test('successfully removes a link', async () => {
      releaseContentRelatedInformationService.update.mockResolvedValue([]);

      const { user } = render(
        <EditingContextProvider editingMode="edit">
          <RelatedPagesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(screen.getByRole('button', { name: 'Edit pages' }));

      const modal = within(screen.getByRole('dialog'));

      await user.click(
        modal.getByRole('button', {
          name: 'Remove Related information description',
        }),
      );

      waitFor(() =>
        expect(
          modal.queryByText('Related information description'),
        ).not.toBeInTheDocument(),
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
        screen.queryByRole('button', { name: 'Add related page' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Edit pages' }),
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
        screen.queryByRole('button', { name: 'Add related page' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Edit pages' }),
      ).not.toBeInTheDocument();
      expect(screen.queryByRole('listitem')).not.toBeInTheDocument();
    });
  });
});
