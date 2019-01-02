import React, { Component } from 'react';
import Breadcrumbs from '../components/breadcrumbs';
import API from '../api';

class Publication extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        const { publication } = this.props.match.params;
        API.get(`publication/${publication}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        return (
            <div>
                <Breadcrumbs current={data.title} />
                <div className="govuk-grid-row">
                    <div className="govuk-grid-column-two-thirds">
                        <strong className="govuk-tag">
                            This is the latest data
                        </strong>
                        <h1 className="govuk-heading-l">
                            {data.title}
                        </h1>
                    </div>
                    <div className="govuk-grid-column-one-third">
                        <aside className="app-related-items">
                            <h3 className="govuk-heading-m" id="subsection-title">
                                About this data
                            </h3>
                            <h3 className="govuk-heading-s">
                                <span className="govuk-caption-m">Published: </span>
                                {/* 22 March 2018  */}
                            </h3>
                            <h2 className="govuk-heading-s">
                                <span className="govuk-caption-m">Next update: </span>
                                    {data.nextUpdate}
                                <span className="govuk-caption-m">
                                    <a href="#notify">Notify me</a>
                                </span>
                            </h2>

                            <hr className="govuk-section-break govuk-section-break--l govuk-section-break--visible" />
                            
                            <h2 className="govuk-heading-m" id="getting-the-data">
                                Getting the data
                            </h2>
                            <ul className="govuk-list">
                                <li><a href="#download" className="govuk-link">Download pdf files</a></li>
                                <li><a href="#download" className="govuk-link">Download .csv files</a></li>
                                <li><a href="#api" className="govuk-link">Access API</a></li>
                            </ul>
                        </aside>
                    </div>
                </div>
            </div>
        );
    }
}

export default Publication;