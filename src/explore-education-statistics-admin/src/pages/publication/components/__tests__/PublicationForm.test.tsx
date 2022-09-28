import PublicationForm, {
  FormValues,
} from '@admin/pages/publication/components/PublicationForm';
import { Release } from '@admin/services/releaseService';
import _themeService, { Theme } from '@admin/services/themeService';
import _publicationService, {
  MyPublication, // @MarkFix remove this
} from '@admin/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/themeService');
const themeService = _themeService as jest.Mocked<typeof _themeService>;
jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationForm', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      slug: 'theme-1',
      title: 'Theme 1',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          slug: 'topic-1',
          title: 'Topic 1',
          themeId: 'theme-1',
        },
        {
          id: 'topic-2',
          slug: 'topic-2',
          title: 'Topic 2',
          themeId: 'theme-1',
        },
      ],
    },
    {
      id: 'theme-2',
      slug: 'theme-2',
      title: 'Theme 2',
      summary: '',
      topics: [
        {
          id: 'topic-3',
          slug: 'topic-3',
          title: 'Topic 3',
          themeId: 'theme-2',
        },
        {
          id: 'topic-4',
          slug: 'topic-4',
          title: 'Topic 4',
          themeId: 'theme-2',
        },
      ],
    },
  ];

  test('does not render any theme/topic fields when no `initialValues`', async () => {
    render(<PublicationForm onSubmit={noop} showTitleInput />);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    expect(screen.queryByLabelText('Select theme')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Select topic')).not.toBeInTheDocument();
  });

  test('shows validation error when there is no title', async () => {
    render(<PublicationForm onSubmit={noop} showTitleInput />);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    userEvent.click(screen.getByLabelText('Publication title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a publication title', {
          selector: '#publicationForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error when there is no summary', async () => {
    render(<PublicationForm onSubmit={noop} showTitleInput />);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication summary')).toBeInTheDocument();
    });

    userEvent.click(screen.getByLabelText('Publication summary'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a publication summary', {
          selector: '#publicationForm-summary-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation errors when there are no contact details', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Team name')).toBeInTheDocument();
    });

    userEvent.click(screen.getByLabelText('Team name'));
    userEvent.tab();

    userEvent.click(screen.getByLabelText('Team email address'));
    userEvent.tab();

    userEvent.click(screen.getByLabelText('Contact name'));
    userEvent.tab();

    userEvent.click(screen.getByLabelText('Contact telephone number'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a team name', {
          selector: '#publicationForm-teamName-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a team email address', {
          selector: '#publicationForm-teamEmail-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a contact name', {
          selector: '#publicationForm-contactName-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a contact telephone number', {
          selector: '#publicationForm-contactTelNo-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when contact email is not valid', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Team email address')).toBeInTheDocument();
    });

    await userEvent.type(
      screen.getByLabelText('Team email address'),
      'not a valid email',
    );
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid team email address', {
          selector: '#publicationForm-teamEmail-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    render(<PublicationForm onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Save publication' }),
      ).toBeInTheDocument();
    });

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    render(<PublicationForm onSubmit={handleSubmit} showTitleInput />);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await userEvent.type(
      screen.getByLabelText('Publication title'),
      'Test title',
    );

    await userEvent.type(
      screen.getByLabelText('Publication summary'),
      'Test summary',
    );

    await userEvent.type(screen.getByLabelText('Team name'), 'Test team');
    await userEvent.type(
      screen.getByLabelText('Team email address'),
      'team@test.com',
    );
    await userEvent.type(screen.getByLabelText('Contact name'), 'John Smith');
    await userEvent.type(
      screen.getByLabelText('Contact telephone number'),
      '0123456789',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Test title',
        summary: 'Test summary',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactName: 'John Smith',
        contactTelNo: '0123456789',
      } as FormValues);
    });
  });

  describe('with `initialValues`', () => {
    test('renders correctly with selected theme and topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            summary: 'Test summary',
            topicId: 'topic-4',
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        const themeSelect = screen.getByLabelText('Select theme');
        const themes = within(themeSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];

        expect(themes).toHaveLength(2);

        expect(themes[0]).toHaveTextContent('Theme 1');
        expect(themes[0].selected).toBe(false);

        expect(themes[1]).toHaveTextContent('Theme 2');
        expect(themes[1].selected).toBe(true);

        const topicSelect = screen.getByLabelText('Select topic');
        const topics = within(topicSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];

        expect(topics).toHaveLength(2);

        expect(topics[0]).toHaveTextContent('Topic 3');
        expect(topics[0].selected).toBe(false);

        expect(topics[1]).toHaveTextContent('Topic 4');
        expect(topics[1].selected).toBe(true);
      });
    });

    test('can successfully submit without any changes', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);

      const initialValues: FormValues = {
        title: 'Test title',
        summary: 'Test summary',
        topicId: 'topic-4',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactTelNo: '0123456789',
        contactName: 'John Smith',
      };

      const handleSubmit = jest.fn();

      render(
        <PublicationForm
          initialValues={initialValues}
          onSubmit={handleSubmit}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Save publication' }),
        ).toBeInTheDocument();
      });

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(initialValues);
      });
    });

    test('can successfully submit with updated topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);

      const initialValues: FormValues = {
        title: 'Test title',
        summary: 'Test summary',
        topicId: 'topic-4',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactTelNo: '0123456789',
        contactName: 'John Smith',
      };

      const handleSubmit = jest.fn();

      render(
        <PublicationForm
          initialValues={initialValues}
          onSubmit={handleSubmit}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Select topic')).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Save publication' }),
        ).toBeInTheDocument();
      });

      userEvent.selectOptions(screen.getByLabelText('Select topic'), [
        'topic-3',
      ]);

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          ...initialValues,
          topicId: 'topic-3',
        });
      });
    });

    test('shows a confirmation modal on submit if the confirmOnSubmit flag is set', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);

      const initialValues: FormValues = {
        title: 'Test title',
        summary: 'Test summary',
        topicId: 'topic-4',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactTelNo: '0123456789',
        contactName: 'John Smith',
      };

      const handleSubmit = jest.fn();

      render(
        <PublicationForm
          initialValues={initialValues}
          onSubmit={handleSubmit}
          confirmOnSubmit
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Select topic')).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Save publication' }),
        ).toBeInTheDocument();
      });

      userEvent.selectOptions(screen.getByLabelText('Select topic'), [
        'topic-3',
      ]);

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

      await waitFor(() => {
        const modal = within(screen.getByRole('dialog'));
        expect(modal.getByRole('heading')).toHaveTextContent(
          'Confirm publication changes',
        );
        userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      });

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          ...initialValues,
          topicId: 'topic-3',
        });
      });
    });
  });

  describe('archive publication', () => {
    const testSingleTheme: Theme = {
      id: 'theme-1',
      slug: 'theme-1',
      title: 'Theme 1',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          slug: 'topic-1',
          title: 'Topic 1',
          themeId: 'theme-1',
        },
      ],
    };

    const testRelease1: Release = {
      amendment: false,
      approvalStatus: 'Approved',
      contact: {
        contactName: 'Test contact name',
        contactTelNo: 'Test contact tel',
        teamEmail: 'test-contact@hiveit.co.uk',
        teamName: 'Test team name',
      },
      id: 'release-id-1',
      latestInternalReleaseNote: 'Test internal release note',
      latestRelease: true,
      live: true,
      permissions: {
        canAddPrereleaseUsers: false,
        canUpdateRelease: false,
        canDeleteRelease: false,
        canMakeAmendmentOfRelease: false,
      },
      preReleaseAccessList: '',
      previousVersionId: 'prev-version-id-1',
      publicationId: 'publication-id-1',
      publicationTitle: 'Publication 1',
      publicationSummary: 'Publication 1 summary',
      publicationSlug: 'publication-slug-1',
      published: '2021-01-01T11:21:17',
      slug: 'release-slug-1',
      timePeriodCoverage: {
        label: 'Academic year',
        value: 'AY',
      },
      title: 'Release 1',
      type: 'AdHocStatistics',
      year: 2021,
      yearTitle: '2021/22',
    };

    const testPublication1: MyPublication = {
      id: 'publication-id-1',
      title: 'Publication 1',
      summary: 'Publcation 1 summary',
      contact: {
        contactName: 'John Smith',
        contactTelNo: '0777777777',
        teamEmail: 'john.smith@test.com',
        teamName: 'Team Smith',
      },
      releases: [testRelease1],
      methodologies: [],
      themeId: 'theme-1',
      topicId: 'topic-1',
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationTitle: true,
        canUpdatePublicationSupersededBy: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canUpdateContact: true,
      },
    };

    const testPublication2: MyPublication = {
      ...testPublication1,
      id: 'publication-id-2',
      title: 'Publication 2',
      releases: [],
    };

    test('does not render the `Archive publication` field when showSupersededBy is false', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.getPublicationSummaries.mockResolvedValue([
        {
          id: testPublication1.id,
          title: testPublication1.title,
          slug: 'slug-1',
        },
      ]);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            summary: 'Test summary',
            topicId: 'topic-4',
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          showTitleInput
          showSupersededBy={false}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      expect(
        screen.queryByLabelText('Superseding publication'),
      ).not.toBeInTheDocument();
    });

    test('renders the `Archive publication` field when showSupersededBy is true', async () => {
      themeService.getThemes.mockResolvedValue([testSingleTheme]);
      publicationService.getPublicationSummaries.mockResolvedValue([
        {
          id: testPublication1.id,
          title: testPublication1.title,
          slug: 'slug-1',
        },
      ]);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            summary: 'Test summary',
            topicId: 'topic-4',
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          showSupersededBy
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Superseding publication'),
        ).toBeInTheDocument();
      });

      const publicationSelect = screen.getByLabelText(
        'Superseding publication',
      );

      const publications = within(publicationSelect).getAllByRole(
        'option',
      ) as HTMLOptionElement[];

      expect(publications).toHaveLength(2);
      expect(publications[0]).toHaveTextContent('None selected');
      expect(publications[1]).toHaveTextContent('Publication 1');
    });

    test('shows correct publications in the superseded by select', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.getPublicationSummaries.mockResolvedValueOnce([
        { ...testPublication1, slug: 'slug-1' },
        { ...testPublication2, slug: 'slug-2' },
      ]);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            summary: 'Test summary',
            topicId: 'topic-4',
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          showSupersededBy
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Superseding publication'),
        ).toBeInTheDocument();
      });

      const publicationSelect = screen.getByLabelText(
        'Superseding publication',
      );

      const publications = within(publicationSelect).getAllByRole(
        'option',
      ) as HTMLOptionElement[];

      expect(publications).toHaveLength(3);
      expect(publications[0]).toHaveTextContent('None selected');
      expect(publications[1]).toHaveTextContent('Publication 1');
      expect(publications[2]).toHaveTextContent('Publication 2');
    });
  });
});
