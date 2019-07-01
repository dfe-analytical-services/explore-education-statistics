import React, { Component } from 'react';

interface IndexResult {
  element: Element;
  location: string;
}

interface Props {
  fromId: string;
}

interface State {
  indexResults: IndexResult[];
  indexComplete: boolean;
}

class ContentSectionIndex extends Component<Props> {
  public state: State = {
    indexResults: [],
    indexComplete: false,
  };

  public componentDidMount() {
    const { fromId } = this.props;
    this.index(fromId);
  }

  private static getLocationText(element: HTMLElement): string {
    const location: string[] = [];
    location.unshift(element.textContent || '');
    return location.join(' > ');
  }

  private index = (fromId: string) => {
    const elements = Array.from(
      document.querySelectorAll(`#${fromId} h2, #${fromId} h3, #${fromId} h4`),
    ) as HTMLElement[];
    const indexResults =
      elements != null
        ? elements.map(element => {
            const location = ContentSectionIndex.getLocationText(element);

            return {
              element,
              location,
            };
          })
        : [];

    this.setState({ indexResults, indexComplete: true });
  };

  public render() {
    const { indexResults, indexComplete } = this.state;

    return (
      <>
        {indexComplete && <h3 className="govuk-heading-s">In this section</h3>}

        {indexResults.length > 0 && (
          <ul className="govuk-body-s">
            {indexComplete &&
              indexResults.map((item, index) => {
                return (
                  <>
                    {item.location && (
                      <li>
                        <a
                          href="#"
                          onClick={e => {
                            e.preventDefault();
                            indexResults[index].element.scrollIntoView();
                          }}
                        >
                          {item.location}
                        </a>
                      </li>
                    )}
                  </>
                );
              })}
          </ul>
        )}
      </>
    );
  }
}

export default ContentSectionIndex;
