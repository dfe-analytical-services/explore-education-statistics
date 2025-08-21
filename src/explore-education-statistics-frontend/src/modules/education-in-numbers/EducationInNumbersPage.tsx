import educationInNumbersService, {
  EducationInNumbersNavItem,
  EducationInNumbersPage,
} from '@frontend/services/educationInNumbersService';
import Page from '@frontend/components/Page';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import { format } from 'date-fns';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import Link from '@frontend/components/Link';
import EducationInNumbersContentSection from './components/EducationInNumbersContentSection';

interface Props {
  pageData: EducationInNumbersPage;
  educationInNumbersPageList: EducationInNumbersNavItem[];
}

const EducationInNumbersPageComponent: NextPage<Props> = ({
  pageData,
  educationInNumbersPageList,
}) => {
  return (
    <Page
      title={pageData.title}
      metaTitle={`${pageData.title}`}
      description={pageData.description}
      breadcrumbs={
        pageData.slug
          ? [{ name: 'Education in Numbers', link: '/education-in-numbers' }]
          : undefined
      }
      caption={format(new Date(pageData.published), 'MMMM yyyy')}
    >
      <div className="govuk-grid-row govuk-!-margin-bottom-3">
        <div className="govuk-grid-column-one-third">
          <nav aria-label="Education in Numbers pages">
            <ul>
              {educationInNumbersPageList.map(page => (
                <li key={page.id}>
                  <Link
                    to={`/education-in-numbers/${page.slug}`}
                    className="govuk-link govuk-link--no-visited-state govuk-link--no-underline"
                  >
                    {page.title}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        </div>
        <div className="govuk-grid-column-two-thirds">
          {pageData.content.map(({ heading, order, content }, index) => {
            return (
              <EducationInNumbersContentSection
                content={content}
                heading={heading}
                isLastSection={index === pageData.content.length - 1}
                key={order}
              />
            );
          })}
        </div>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { slug = '' } = query;

    const [pageData, educationInNumbersPageList] = await Promise.all([
      educationInNumbersService.getEducationInNumbersPage(slug as string),
      educationInNumbersService.listEducationInNumbersPages(),
    ]);
    return {
      props: {
        pageData,
        educationInNumbersPageList,
      },
    };
  },
);

export default EducationInNumbersPageComponent;
