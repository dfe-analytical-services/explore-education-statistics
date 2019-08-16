/* eslint-disable react/destructuring-assignment,prefer-destructuring,no-param-reassign */

import EditableAccordion from '@admin/components/EditableAccordion';
import EditableAccordionSection from '@admin/components/EditableAccordionSection';
import Link from '@admin/components/Link';
import EditableContentBlock from '@admin/modules/find-statistics/components/EditableContentBlock';
import PrototypeEditableContent from '@admin/pages/prototypes/components/PrototypeEditableContent';
import PrototypeDataSample from '@admin/pages/prototypes/publication/components/PrototypeDataSample';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import RelatedAside from '@common/components/RelatedAside';
import React, { Component } from 'react';
import {
  DragDropContext,
  Draggable,
  DraggableLocation,
  Droppable,
  DropResult,
} from 'react-beautiful-dnd';
import { EditableRelease } from '@admin/services/publicationService';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
} from '@common/components/form';
import AddComment from '@admin/pages/prototypes/components/PrototypeEditableContentAddComment';
import ResolveComment from '@admin/pages/prototypes/components/PrototypeEditableContentResolveComment';

interface State {
  reordering: boolean;
  data?: EditableRelease | undefined;
}

interface Props {
  editing?: boolean;
  higherReview?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  data: EditableRelease | undefined;
}

const ACCORDION_ID_REGEXP = /accordion[(]([0-9]+)[)]/;

function getAccordionIndex(id: string | null | undefined) {
  if (id) {
    const regexpResult = ACCORDION_ID_REGEXP.exec(id);
    if (regexpResult) return regexpResult[1];
  }
  return null;
}

class EditablePublicationPage extends Component<Props, State> {
  public static defaultProps = { editing: false };

  public constructor(props: Props) {
    super(props);

    this.state = { reordering: false, data: undefined };
  }

  public componentDidMount() {
    const { data } = this.props;
    this.setState({ data });
  }

  public onDragEnd = (result: DropResult) => {
    if (result.destination) {
      const { source } = result;
      const target: DraggableLocation = result.destination;

      const { data } = this.state;

      if (result.type === 'accordion') {
        const sourceAccordion = getAccordionIndex(source.droppableId);
        const targetAccordion = getAccordionIndex(target.droppableId);

        if (sourceAccordion && targetAccordion) {
          if (source.index !== target.index) {
            if (data) {
              const release: EditableRelease = (data as unknown) as EditableRelease;

              const resultList = Array.from(release.content);

              const [moved] = resultList.splice(source.index, 1);
              resultList.splice(target.index, 0, moved);

              data.content = resultList;

              this.setState({ data: release });
            }
          }
        }
      }
    }
  };

  public addNewSection() {
    if (this.state.data) {
      const { data } = this.state;

      data.content.push({
        heading: 'New section',
        order: data.content.length + 1,
        caption: '',
        content: [
          {
            type: 'MarkDownBlock',
            body: 'editable',
            comments: [],
          },
        ],
      });

      this.setState({ data });
    }
  }

  private renderContentSections(
    data: EditableRelease,
    editing: boolean | undefined,
    reviewing: boolean | undefined,
    resolveComments: boolean | undefined,
  ) {
    const { reordering } = this.state;

    if (reordering) {
      return this.renderDraggableSections(data);
    }
    return this.renderEditableSections(
      data,
      editing,
      reviewing,
      resolveComments,
    );
  }

  private renderEditableSections(
    data: EditableRelease,
    editing: boolean | undefined,
    reviewing: boolean | undefined,
    resolveComments: boolean | undefined,
  ) {
    return (
      <div>
        <h2 className="govuk-heading-l reorderable-relative">
          <button
            className="govuk-button govuk-button--secondary reorderable"
            onClick={() => this.setState({ reordering: true })}
            type="button"
          >
            Reorder sections
          </button>
          Contents
        </h2>

        <EditableAccordion id="contents-sections" index={0}>
          {data.content.map(({ heading, caption, order, content }, index) => (
            <EditableAccordionSection
              heading={heading}
              caption={caption}
              index={index}
              key={`${order}_${heading}`}
            >
              <EditableContentBlock
                editable={editing}
                content={content}
                reviewing={reviewing}
                resolveComments={resolveComments}
                id={`editable-block-${index}`}
                onContentChange={(block, newContent) => {
                  block.body = newContent;
                }}
              />
            </EditableAccordionSection>
          ))}
        </EditableAccordion>

        <h2 className="govuk-heading-l reorderable-relative">
          <button
            type="button"
            className="govuk-button reorderable"
            onClick={() => this.addNewSection()}
          >
            Add new section
          </button>
        </h2>
      </div>
    );
  }

  private renderDraggableSections(data: EditableRelease) {
    return (
      <DragDropContext onDragEnd={this.onDragEnd}>
        <h2 className="govuk-heading-l reorderable-relative">
          <button
            className="govuk-button reorderable"
            onClick={() => this.setState({ reordering: false })}
            type="button"
          >
            Save reordering
          </button>
          Contents
        </h2>

        <div className="govuk-accordion__controls">&nbsp;</div>

        <Droppable droppableId="accordion(0)" type="accordion">
          {droppableProvided => (
            <div
              {...droppableProvided.droppableProps}
              ref={droppableProvided.innerRef}
            >
              {data &&
                data.content.map(({ heading, order }, index) => (
                  <Draggable
                    draggableId={`section(${order})`}
                    index={index}
                    key={`${order}`}
                  >
                    {draggableProvided => (
                      <div
                        className="govuk-accordion__section"
                        ref={draggableProvided.innerRef}
                        {...draggableProvided.draggableProps}
                      >
                        <div className="govuk-accordion__section-header">
                          <h2 className="govuk-accordion__section-heading reorderable-relative">
                            <span
                              className="drag-handle"
                              {...draggableProvided.dragHandleProps}
                            />
                            <button
                              className="govuk-accordion__section-button"
                              type="button"
                            >
                              {heading}
                            </button>
                          </h2>
                        </div>
                      </div>
                    )}
                  </Draggable>
                ))}

              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      </DragDropContext>
    );
  }

  public render() {
    const { reviewing } = this.props;
    const { higherReview } = this.props;
    const { resolveComments } = this.props;
    const { editing } = this.props;
    const { data } = this.state;

    return (
      <>
        <div className={editing ? 'page-editing' : ''}>
          <h1 className="govuk-heading-l">
            <span className="govuk-caption-l">Academic year 2016/17</span>
            Pupil absence statistics and data for schools in England
          </h1>

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-three-quarters">
                  <span className="govuk-tag">
                    {higherReview ? 'Ready for sign-off' : 'In draft'}
                  </span>

                  <dl className="dfe-meta-content">
                    <dt className="govuk-caption-m">Publish date: </dt>
                    <dd>
                      <strong>20 September 2019</strong>
                    </dd>
                    <div>
                      <dt className="govuk-caption-m">Next update: </dt>
                      <dd>
                        <strong>20 September 2020</strong>
                      </dd>
                    </div>
                  </dl>
                </div>

                <div className="govuk-grid-column-one-quarter">
                  <img
                    src="/static/images/UKSA-quality-mark.jpg"
                    alt="UK statistics authority quality mark"
                    height="120"
                    width="120"
                  />
                </div>
              </div>

              {reviewing && data && (
                <AddComment initialComments={data.keyStatistics.comments} />
              )}
              {resolveComments && data && (
                <ResolveComment initialComments={data.keyStatistics.comments} />
              )}
              <PrototypeEditableContent
                reviewing={reviewing}
                resolveComments={resolveComments}
                editable={editing}
                content={`
          <p className="govuk-body">
            Read national statistical summaries and definitions, view charts and
            tables and download data files across a range of pupil absence
            subject areas.
          </p>
          <p className="govuk-body">
            You can also view a regional breakdown of statistics and data within
            the
            <a href="#contents-sections-heading-9">
              <strong>local authorities section</strong>
            </a>
          </p>
          `}
              />
              <p>
                <Link to="/prototypes/methodology-absence">
                  Find out how and why these statistics are collected and
                  published -{' '}
                  <Link to="#">Pupil absence statistics: methodology</Link>
                </Link>
              </p>
              <Details summary="Download underlying data files">
                <ul className="govuk-list">
                  <li>
                    <a href="#" className="govuk-link">
                      Download pdf files
                    </a>
                  </li>
                  <li>
                    <a href="#" className="govuk-link">
                      Download Excel files
                    </a>
                  </li>
                  <li>
                    <a href="#" className="govuk-link">
                      Download .csv files
                    </a>
                  </li>
                  <li>
                    <a href="#" className="govuk-link">
                      Access API
                    </a>
                  </li>
                </ul>
              </Details>
            </div>

            <div className="govuk-grid-column-one-third">
              <RelatedAside>
                <h2 className="govuk-heading-m" id="subsection-title">
                  About these statistics
                </h2>

                <dl className="dfe-meta-content">
                  <dt className="govuk-caption-m">For school year:</dt>
                  <dd data-testid="release-name">
                    <strong>2018 to 2019</strong>
                  </dd>

                  <dd>
                    <Details summary="See previous 7 releases">
                      <ul className="govuk-list">
                        <li>
                          <a
                            className="govuk-link"
                            href="/themes/schools/absence-and-exclusions/pupil-absence-in-schools-in-england/2015-16"
                          >
                            2015 to 2016
                          </a>
                        </li>
                        <li>
                          <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015">
                            2014 to 2015
                          </a>
                        </li>
                        <li>
                          <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014">
                            2013 to 2014
                          </a>
                        </li>
                        <li>
                          <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013">
                            2012 to 2013
                          </a>
                        </li>
                        <li>
                          <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics">
                            2011 to 2012
                          </a>
                        </li>
                        <li>
                          <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011">
                            2010 to 2011
                          </a>
                        </li>
                        <li>
                          <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010">
                            2009 to 2010
                          </a>
                        </li>
                      </ul>
                    </Details>
                  </dd>
                </dl>

                <dl className="dfe-meta-content">
                  <dt className="govuk-caption-m">Last updated: </dt>
                  <dd data-testid="last-updated">
                    <strong>19 April 2019</strong>
                  </dd>
                  <dd>
                    <Details summary="See updates">
                      <div data-testid="publication-page--update-element">
                        <h3 className="govuk-heading-s">20 September 2019</h3>
                        <p>First published.</p>
                      </div>
                    </Details>
                  </dd>
                </dl>

                {editing && (
                  <>
                    <FormFieldset id="test" legend="">
                      <FormGroup>
                        <label
                          htmlFor="releaseUpdateNote"
                          className="govuk-label"
                        >
                          Add release note
                        </label>
                        <textarea
                          className="govuk-textarea govuk-!-margin-bottom-3"
                          id="releaseUpdateNote"
                          name="releaseUpdateNote"
                          rows={3}
                          value=""
                        >
                          test
                        </textarea>
                      </FormGroup>
                      <button type="submit" className="govuk-button">
                        Add note
                      </button>
                    </FormFieldset>
                  </>
                )}

                <nav role="navigation" aria-labelledby="related-content">
                  <h2
                    className="govuk-heading-m govuk-!-margin-top-6"
                    id="related-content"
                  >
                    Related information
                  </h2>

                  <ul className="govuk-list">
                    <li>
                      <Link to="/prototypes/methodology-absence">
                        Pupil absence statistics: guidance and methodology
                      </Link>
                    </li>
                    <li>
                      <Link to="/prototypes/methodology-absence">
                        This is an example of a related information link
                      </Link>
                      {editing && (
                        <div>
                          <Link
                            to="#"
                            className="govuk-button govuk-button--secondary"
                          >
                            Remove link
                          </Link>
                        </div>
                      )}
                    </li>
                  </ul>

                  {editing && (
                    <>
                      <FormFieldset id="test" legend="Add related information">
                        <FormGroup>
                          <FormTextInput
                            id="link-title"
                            name="link-title"
                            label="Title"
                            value=""
                          />
                        </FormGroup>
                        <FormGroup>
                          <FormTextInput
                            id="link-url"
                            name="link-url"
                            label="Link url"
                            value=""
                          />
                        </FormGroup>
                      </FormFieldset>
                      <button type="submit" className="govuk-button">
                        Add related information
                      </button>
                    </>
                  )}
                </nav>
              </RelatedAside>
            </div>
          </div>
          <hr />
          <h2 className="govuk-heading-l">
            Latest headline facts and figures - 2018/19
          </h2>
          {reviewing && data && (
            <AddComment initialComments={data.keyStatistics.comments} />
          )}
          {resolveComments && data && (
            <ResolveComment initialComments={data.keyStatistics.comments} />
          )}

          <PrototypeDataSample
            editing={editing}
            reviewing={reviewing}
            sectionId="headlines"
            chartTitle="change in absence types in England"
            xAxisLabel="School Year"
            yAxisLabel="Absence Rate"
            chartData={[
              {
                authorised: 4.2,
                name: '2012/13',
                overall: 5.3,
                unauthorised: 1.1,
              },
              {
                authorised: 3.5,
                name: '2013/14',
                overall: 4.5,
                unauthorised: 1.1,
              },
              {
                authorised: 3.5,
                name: '2014/15',
                overall: 4.6,
                unauthorised: 1.1,
              },
              {
                authorised: 3.4,
                name: '2015/16',
                overall: 4.6,
                unauthorised: 1.1,
              },
              {
                authorised: 3.4,
                name: '2016/17',
                overall: 4.7,
                unauthorised: 1.3,
              },
            ]}
            chartDataKeys={['unauthorised', 'authorised', 'overall']}
          />
          {data &&
            data.content.length > 0 &&
            this.renderContentSections(
              data,
              editing,
              reviewing,
              resolveComments,
            )}
          <h2 className="govuk-heading-m govuk-!-margin-top-9">
            Extra information
          </h2>
          <Accordion id="extra-information-sections">
            <AccordionSection
              heading="Where does this data come from?"
              caption="Our methodology, how we collect and process the data"
              headingTag="h3"
            >
              <ul className="govuk-list">
                <li>
                  <a
                    href="/prototypes/methodology-absence"
                    className="govuk-link"
                  >
                    How do we collect it?
                  </a>
                </li>
                <li>
                  <a
                    href="/prototypes/methodology-absence"
                    className="govuk-link"
                  >
                    What do we do with it?
                  </a>
                </li>
                <li>
                  <a
                    href="/prototypes/methodology-absence"
                    className="govuk-link"
                  >
                    Related policies
                  </a>
                </li>
              </ul>
            </AccordionSection>
            <AccordionSection heading="Feedback and questions" headingTag="h3">
              <ul className="govuk-list">
                <li>
                  <a href="#" className="govuk-link">
                    Feedback on this page
                  </a>
                </li>
                <li>
                  <a href="#" className="govuk-link">
                    Make a suggestion
                  </a>
                </li>
                <li>
                  <a href="#" className="govuk-link">
                    Ask a question
                  </a>
                </li>
              </ul>
            </AccordionSection>
            <AccordionSection heading="Contact us" headingTag="h3">
              <h4 className="govuk-heading-">Media enquiries</h4>
              <address className="govuk-body dfe-font-style-normal">
                Press Office News Desk
                <br />
                Department for Education <br />
                Sanctuary Buildings <br />
                Great Smith Street <br />
                London
                <br />
                SW1P 3BT <br />
                Telephone: 020 7783 8300
              </address>

              <h4 className="govuk-heading-">Other enquiries</h4>
              <address className="govuk-body dfe-font-style-normal">
                Data Insight and Statistics Division
                <br />
                Level 1<br />
                Department for Education
                <br />
                Sanctuary Buildings <br />
                Great Smith Street
                <br />
                London
                <br />
                SW1P 3BT <br />
                Telephone: 020 7783 8300
                <br />
                Email: <a href="#">Schools.statistics@education.gov.uk</a>
              </address>
            </AccordionSection>
          </Accordion>
          {!editing && (
            <>
              <h2 className="govuk-heading-m govuk-!-margin-top-9">
                Exploring the data
              </h2>
              <p>
                The statistics can be viewed as reports, or you can customise
                and download as excel or .csv files . The data can also be
                accessed via an API. <a href="#">What is an API?</a>
              </p>
              <Link to="/prototypes/table-tool" className="govuk-button">
                Create charts and tables
              </Link>
              <div className="govuk-!-margin-top-9">
                <a href="#print" className="govuk-link">
                  Print this page
                </a>
              </div>
            </>
          )}
        </div>
      </>
    );
  }
}

export default EditablePublicationPage;
