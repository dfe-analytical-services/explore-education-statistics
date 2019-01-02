import React, { Component } from 'react';
import DataList from '../components/datalist';
import Title from '../components/title';
import API from '../api';

class Themes extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        API.get('theme')
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label='Themes' />
                    <DataList data={data} linkIdentifier={window.location.pathname} />
                </div>
            </div>
        );
    }
}

export default Themes;