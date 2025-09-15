import Page from '@frontend/components/Page';
import SubNav from '@frontend/components/SubNav';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import EducationInNumbersContentSection from '@frontend/modules/education-in-numbers/components/EducationInNumbersContentSection';
import educationInNumbersService, {
  EinNavItem,
  EinPage,
} from '@frontend/services/educationInNumbersService';
import { format } from 'date-fns';
import { GetServerSideProps, NextPage } from 'next';
import Head from 'next/head';
import React from 'react';

interface Props {
  pageData: EinPage;
  educationInNumbersPageList: EinNavItem[];
}

const EducationInNumbersPage: NextPage<Props> = ({
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
      <Head>
        {/* EES-6497 Remove Head so EiN pages are indexed by search engines */}
        <meta name="robots" content="noindex,nofollow" />
      </Head>
      <div className="govuk-grid-row govuk-!-margin-bottom-3">
        <SubNav
          headingVisible={false}
          items={educationInNumbersPageList.map(page => {
            return {
              href: `/education-in-numbers/${
                page.slug === undefined ? '' : page.slug
              }`,
              isActive: page.slug === pageData.slug,
              slug: page.slug ?? '',
              text: page.title,
            };
          })}
        />
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

export default EducationInNumbersPage;
