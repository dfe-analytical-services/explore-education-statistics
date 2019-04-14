import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import PrototypeDataTilesHighlights
  from '@common/prototypes/publication/components/PrototypeDataTilesHighlights';
import PrototypeTableSample from '@common/prototypes/publication/components/PrototypeTableSample';
import React from 'react';
import { PrototypeEditableContent } from 'src/pages/prototypes/components/PrototypeEditableContent';
import PrototypeChartSample from 'src/pages/prototypes/publication/components/PrototypeChartSample';

interface Props {
  sectionId?: string;
  chartTitle?: string;
  xAxisLabel?: string;
  yAxisLabel?: string;
  chartData?: object[];
  chartDataKeys: string[];
}

const PrototypeDataSample = ({
  sectionId,
  chartTitle,
  xAxisLabel,
  yAxisLabel,
  chartData,
  chartDataKeys,
}: Props) => {
  return (
    <>
      <Tabs>
        {sectionId === 'headlines' && (
          <TabsSection id={`${sectionId}SummaryData`} title="Summary">
            <PrototypeDataTilesHighlights />
            <PrototypeEditableContent
              content={`
            <ul className="govuk-list govuk-list--bullet">
              <li>pupils missed on average 8.2 school days</li>
              <li>
                overall and unauthorised absence rates up on previous year
              </li>
              <li>
                unauthorised rise due to higher rates of unauthorised holidays
              </li>
              <li>10% of pupils persistently absent during 2016/17</li>
            </ul>
          `}
            />
          </TabsSection>
        )}
        <TabsSection id={`${sectionId}TableData`} title="Data tables">
          <PrototypeTableSample caption={`Table showing ${chartTitle}`} />
        </TabsSection>
        <TabsSection id={`${sectionId}ChartData`} title="Charts">
          <h2 className="govuk-heading-s">{`Chart showing ${chartTitle}`}</h2>
          <PrototypeChartSample
            xAxisLabel={xAxisLabel}
            yAxisLabel={yAxisLabel}
            chartData={chartData}
            chartDataKeys={chartDataKeys}
          />
        </TabsSection>
        <TabsSection id={`${sectionId}Downloads`} title="Data downloads">
          <h2 className="govuk-heading-s">
            Download overall absence data files
          </h2>
          <ul className="govuk-list">
            <li>
              <a className="govuk-link" href="#">
                Excel table
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                csv
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                json
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                API
              </a>
            </li>
          </ul>
        </TabsSection>
      </Tabs>
    </>
  );
};

export default PrototypeDataSample;
