import React, { Component } from 'react';
import DataList from '../components/datalist';
import API from '../api';
import Breadcrumbs from '../components/breadcrumbs';
import Title from '../components/title';
import Glink from '../components/glink';
class Topic extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: [],
            publications: []
        }
    }

    componentDidMount() {
        const { topic } = this.props.match.params;
        API.get(`topic/${topic}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
        API.get(`topic/${topic}/publications`)
            .then(json => this.setState({ publications: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        const { publications } = this.state;
        return (
            <div>
                <Breadcrumbs current={data.title} />
                <div className="govuk-grid-row">
                    <div className="govuk-grid-column-two-thirds">
                        <Title label={data.title} />

                        <p className="govuk-body">Here you can find DfE stats for {(data.title || '').toLowerCase()}, and access them as reports, customise and download as excel files or csv files, and access them via an API. <Glink>(Find out more)</Glink></p>
                        <p className="govuk-body">You can also see our statistics for 16+ education and social care.</p>

                        <h3 className="govuk-heading-m">The following publications are available in {(data.title || '').toLowerCase()}</h3>
                        <DataList data={publications} linkIdentifier={window.location.pathname} />
                    </div>
                </div>
                <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible"/>
                
                <section id="latest-publications">
                    <h2 className="govuk-heading-l">Latest publications in {(data.title || '').toLowerCase()}</h2>
                    <p className="govuk-body">These are the latest official statistics with figures in {(data.title || '').toLowerCase()}. You can access the report and commentary, and also get the data for use in Excel and other tools. You can now customise the data to your requirements, and get a variety of formats.</p>
                    <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible"/>
                </section>

                <section id="key-indicators">
                    <h2 className="govuk-heading-l">Key indicators for {(data.title || '').toLowerCase()}</h2>
                    <p className="govuk-body">These are some key indicators for {(data.title || '').toLowerCase()}. You can change what you see here according to your requirements.</p>
                    <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible"/>
                </section>

                <section id="explore-statistics">
                    <h2 className="govuk-heading-l">Explore {(data.title || '').toLowerCase()} statistics</h2>
                    <ul className="govuk-list govuk-list--bullet">
                            <li>You can explore all the DfE statistics available for {(data.title || '').toLowerCase()} here. You can use our step by step guide, or dive straight in.</li>
                            {/* <li>Once you've chosen your data you can view it by ###.</li> */}
                            <li>You can also dowload it, visualise it or copy and paste it as you need.</li>
                        </ul>
                </section>
            </div>
        );
    }
}

export default Topic;