import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService, {
  MethodologyStatusListItem,
} from '@admin/services/methodologyService';
import InsetText from '@common/components/InsetText';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import Tag from '@common/components/Tag';
import React, { useEffect, useState } from 'react';
import { generatePath } from 'react-router';

interface Model {
  liveMethodologies: MethodologyStatusListItem[];
  approvedMethodologies: MethodologyStatusListItem[];
  draftMethodologies: MethodologyStatusListItem[];
}

interface MethodologiesTableProps {
  methodologies: MethodologyStatusListItem[];
}

const MethodologiesTable = ({ methodologies }: MethodologiesTableProps) => {
  return (
    <table className="govuk-table">
      <thead className="govuk-table__head">
        <tr className="govuk-table__row">
          <th scope="col" className="govuk-table__header">
            Methodology title
          </th>
          <th scope="col" className="govuk-table__header">
            Status
          </th>
          <th scope="col" className="govuk-table__header">
            Related publications
          </th>
        </tr>
      </thead>
      <tbody className="govuk-table__body">
        {methodologies.map(methodology => (
          <tr className="govuk-table__row" key={methodology.id}>
            <td className="govuk-table__header">
              <Link
                to={generatePath<MethodologyRouteParams>(
                  methodologySummaryRoute.path,
                  {
                    methodologyId: methodology.id,
                  },
                )}
              >
                {methodology.title}
              </Link>
            </td>
            <td className="govuk-table__cell">
              <Tag strong>{methodology.status}</Tag>
            </td>
            <td className="govuk-table__cell">
              <ul className="govuk-list">
                {methodology.publications.map(publication => (
                  <li key={publication.id}>{publication.title}</li>
                ))}
              </ul>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

const MethodologiesPage = () => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    methodologyService.getMyMethodologies().then(methodologies => {
      const liveMethodologies: MethodologyStatusListItem[] = methodologies.filter(
        methodology => {
          return methodology.status === 'Live';
        },
      );
      const approvedMethodologies: MethodologyStatusListItem[] = methodologies.filter(
        methodology => {
          return methodology.status === 'Approved';
        },
      );
      const draftMethodologies: MethodologyStatusListItem[] = methodologies.filter(
        methodology => {
          return methodology.status === 'Draft';
        },
      );
      setModel({
        liveMethodologies,
        approvedMethodologies,
        draftMethodologies,
      });
    });
  }, []);

  return (
    <Page wide breadcrumbs={[{ name: 'Manage methodologies' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">Manage methodologies</h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation" target="blank">
                  Creating new methodology{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <Tabs id="methodologyTabs">
        <TabsSection
          id="live-methodologies"
          title={`Live methodologies ${
            model && model.liveMethodologies.length
              ? `(${model.liveMethodologies.length})`
              : '(0)'
          }`}
        >
          <Link
            to="/methodologies/create"
            className="govuk-button govuk-!-margin-bottom-1"
          >
            Create new methodology
          </Link>
          {model && model.liveMethodologies.length ? (
            <MethodologiesTable methodologies={model.liveMethodologies} />
          ) : (
            <InsetText>There are currently no live methodologies</InsetText>
          )}
          <Link to="/methodologies/create" className="govuk-button">
            Create new methodology
          </Link>
        </TabsSection>
        <TabsSection
          id="draft-methodologies"
          title={`Draft methodologies ${
            model && model.draftMethodologies.length
              ? `(${model.draftMethodologies.length})`
              : '(0)'
          }`}
        >
          {model && model.draftMethodologies.length ? (
            <MethodologiesTable methodologies={model.draftMethodologies} />
          ) : (
            <InsetText>There are currently no draft methodologies</InsetText>
          )}
        </TabsSection>
        <TabsSection
          id="approved-methodologies"
          title={`Approved methodologies ${
            model && model.approvedMethodologies.length
              ? `(${model.approvedMethodologies.length})`
              : '(0)'
          }`}
        >
          {model && model.approvedMethodologies.length ? (
            <MethodologiesTable methodologies={model.approvedMethodologies} />
          ) : (
            <InsetText>There are currently no approved methodologies</InsetText>
          )}
        </TabsSection>
      </Tabs>
    </Page>
  );
};

export default MethodologiesPage;
