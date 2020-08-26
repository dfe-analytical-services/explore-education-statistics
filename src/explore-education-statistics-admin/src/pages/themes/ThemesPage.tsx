import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import useQueryParams from '@admin/hooks/useQueryParams';
import {
  themeCreateRoute,
  themeEditRoute,
  ThemeParams,
  ThemeTopicParams,
  topicCreateRoute,
  topicEditRoute,
} from '@admin/routes/routes';
import themeService from '@admin/services/themeService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router';

const ThemesPage = () => {
  const { themeId } = useQueryParams<ThemeParams>();

  const { value: themes = [], isLoading } = useAsyncHandledRetry(
    themeService.getThemes,
  );

  return (
    <Page
      title="Manage themes and topics"
      breadcrumbs={[{ name: 'Manage themes and topics' }]}
    >
      <LoadingSpinner loading={isLoading}>
        <Accordion id="themes">
          {themes.map(theme => (
            <AccordionSection
              key={theme.id}
              heading={theme.title}
              open={themeId === theme.id}
            >
              <SummaryList noBorder>
                <SummaryListItem
                  term="Summary"
                  testId={`Summary for ${theme.title}`}
                  actions={
                    <Link
                      data-testid={`Edit link for ${theme.title}`}
                      unvisited
                      to={generatePath<ThemeParams>(themeEditRoute.path, {
                        themeId: theme.id,
                      })}
                    >
                      Edit theme
                    </Link>
                  }
                >
                  {theme.summary}
                </SummaryListItem>
              </SummaryList>

              <section data-testid={`Topics for ${theme.title}`}>
                <h3>Topics</h3>

                {theme.topics.length > 0 ? (
                  <SummaryList>
                    {theme.topics.map(topic => (
                      <SummaryListItem
                        key={topic.id}
                        term={topic.title}
                        actions={
                          <Link
                            data-testid={`Edit ${topic.title} topic link for ${theme.title}`}
                            unvisited
                            to={generatePath<ThemeTopicParams>(
                              topicEditRoute.path,
                              {
                                themeId: theme.id,
                                topicId: topic.id,
                              },
                            )}
                          >
                            Edit topic
                          </Link>
                        }
                      />
                    ))}
                  </SummaryList>
                ) : (
                  <p>No topics for this theme</p>
                )}
              </section>

              <ButtonLink
                data-testid={`Create topic link for ${theme.title}`}
                to={generatePath<ThemeParams>(topicCreateRoute.path, {
                  themeId: theme.id,
                })}
              >
                Create topic
              </ButtonLink>
            </AccordionSection>
          ))}
        </Accordion>

        <ButtonLink to={themeCreateRoute.path}>Create theme</ButtonLink>
      </LoadingSpinner>
    </Page>
  );
};

export default ThemesPage;
