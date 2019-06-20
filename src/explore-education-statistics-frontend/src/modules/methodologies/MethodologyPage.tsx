import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import publicationService, {
  Release,
} from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { NextContext } from 'next';
import React, { Component } from 'react';

interface Props {
  publication: string;
  release: string;
  data: Release;
}

class MethodologyPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    publication: string;
    release: string;
  }>) {
    const { publication, release } = query;

    const request = release
      ? publicationService.getPublicationRelease(release)
      : publicationService.getLatestPublicationRelease(publication);

    const data = await request;

    return {
      data,
      publication,
      release,
    };
  }

  public render() {
    const { data, release } = this.props;

    const releaseCount =
      data.publication.releases.slice(1).length +
      data.publication.legacyReleases.length;

    return (
      <Page
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/statistics' },
          { name: data.title },
        ]}
      >
        <h2>Headline facts and figures - {data.releaseName}</h2>

        {data.keyStatistics && (
          <DataBlock {...data.keyStatistics} id="keystats" />
        )}

        <Accordion id="extra-information-sections">
          <AccordionSection
            heading={`${data.title}: methodology`}
            caption="Find out how and why we collect, process and publish these statistics"
            headingTag="h3"
          >
            <p>
              Read our{' '}
              <Link to={`/methodology/${data.publication.slug}`}>
                {`${data.publication.title}: methodology`}
              </Link>{' '}
              guidance.
            </p>
          </AccordionSection>
        </Accordion>
      </Page>
    );
  }
}

export default MethodologyPage;
