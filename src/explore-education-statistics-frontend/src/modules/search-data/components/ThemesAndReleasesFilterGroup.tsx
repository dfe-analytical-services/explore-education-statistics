import { FormTextSearchInput } from '@common/components/form';
import FormCheckbox from '@common/components/form/FormCheckbox';
import { Theme } from '@common/services/publicationService';
import React, { memo, useMemo, useState } from 'react';
import ThemesAndReleasesFilterGroupReleases from './ThemesAndReleasesFilterGroupReleases';

export interface ThemesAndReleasesFilterGroupProps {
  publicationIds?: string[];
  publicationTree: Theme[];
  themeIds?: string[];
  onChangeBatch: (nextThemeIds: string[], nextPublicationIds: string[]) => void;
}

const ThemesAndReleasesFilterGroup = ({
  publicationIds = [],
  publicationTree,
  themeIds = [],
  onChangeBatch,
}: ThemesAndReleasesFilterGroupProps) => {
  const [searchTerm, setSearchTerm] = useState('');

  // Filter the tree based on the search term
  const filteredTree = useMemo(() => {
    if (!searchTerm) {
      return publicationTree;
    }

    const lowercaseSearchTerm = searchTerm.toLowerCase();

    return publicationTree.reduce<Theme[]>((acc, theme) => {
      const matchesTheme = theme.title
        .toLowerCase()
        .includes(lowercaseSearchTerm);
      const matchingPubs = theme.publications.filter(pub =>
        pub.title.toLowerCase().includes(lowercaseSearchTerm),
      );

      if (matchesTheme || matchingPubs.length > 0) {
        acc.push({
          ...theme,
          publications: matchingPubs,
        });
      }

      return acc;
    }, []);
  }, [publicationTree, searchTerm]);

  // Calculate total matching checkboxes (themes + publications)
  const matchCount = useMemo(() => {
    return filteredTree.reduce(
      (total, theme) => total + 1 + (theme.publications?.length || 0),
      0,
    );
  }, [filteredTree]);

  const handleThemeChange = (themeId: string, isChecked: boolean) => {
    let nextThemeIds = [...themeIds];
    let nextPublicationIds = [...publicationIds];

    if (isChecked) {
      nextThemeIds.push(themeId);
      // Uncheck all publications contained under this theme to avoid logical overlap
      const themePubIds =
        publicationTree
          .find(t => t.id === themeId)
          ?.publications.map(p => p.id) || [];
      nextPublicationIds = nextPublicationIds.filter(
        id => !themePubIds.includes(id),
      );
    } else {
      nextThemeIds = nextThemeIds.filter(id => id !== themeId);
    }

    onChangeBatch(nextThemeIds, nextPublicationIds);
  };

  const handlePublicationChange = (
    themeId: string,
    pubId: string,
    isChecked: boolean,
  ) => {
    let nextThemeIds = [...themeIds];
    let nextPublicationIds = [...publicationIds];

    if (isChecked) {
      nextPublicationIds.push(pubId);
      // Uncheck the parent theme if a nested publication is specifically selected
      nextThemeIds = nextThemeIds.filter(id => id !== themeId);
    } else {
      nextPublicationIds = nextPublicationIds.filter(id => id !== pubId);
    }

    onChangeBatch(nextThemeIds, nextPublicationIds);
  };

  return (
    <div className="govuk-form-group">
      <FormTextSearchInput
        id="theme-publication-search"
        name="theme-publication-search"
        label="Find themes and releases"
        width={20}
        onChange={event => setSearchTerm(event.target.value)}
        onKeyPress={event => {
          if (event.key === 'Enter') {
            event.preventDefault();
          }
        }}
      />

      {searchTerm && (
        <span aria-live="polite" aria-atomic className="govuk-visually-hidden">
          {`${matchCount} option${matchCount === 1 ? '' : 's'} found`}
        </span>
      )}

      <fieldset className="govuk-fieldset govuk-!-margin-top-4">
        <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
          Themes
        </legend>

        {filteredTree.length === 0 ? (
          <p className="govuk-body">No options available.</p>
        ) : (
          <div className="govuk-checkboxes govuk-checkboxes--small">
            {filteredTree.map(theme => {
              const isThemeChecked = themeIds.includes(theme.id);
              const publications = theme.publications || [];
              const hasPublications = publications.length > 0;

              return (
                <div key={theme.id} className="govuk-!-margin-bottom-1">
                  <FormCheckbox
                    id={`theme-${theme.id}`}
                    name="themeId"
                    value={theme.id}
                    label={theme.title}
                    checked={isThemeChecked}
                    onChange={e =>
                      handleThemeChange(theme.id, e.target.checked)
                    }
                  />

                  {hasPublications && (
                    <div className="govuk-!-margin-left-4">
                      <ThemesAndReleasesFilterGroupReleases
                        publicationIds={publicationIds}
                        publications={publications}
                        themeId={theme.id}
                        themeTitle={theme.title}
                        onChange={handlePublicationChange}
                      />
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </fieldset>
    </div>
  );
};

export default memo(ThemesAndReleasesFilterGroup);
