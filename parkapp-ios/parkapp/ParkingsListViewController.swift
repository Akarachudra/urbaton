//
//  ParkingsListViewController.swift
//  parkapp
//
//  Created by Ilya Sedov on 19/05/2019.
//  Copyright © 2019 SKB Kontur. All rights reserved.
//

import UIKit

struct Parking: Decodable {
  let number: Int
  let title: String
  let total: Int
  let vacant: Int
  let occupied: Int

  enum CodingKeys: String, CodingKey {
    case number = "CameraNumber"
    case title = "Description"
    case total = "TotalPlaces"
    case vacant = "FreePlaces"
    case occupied = "OccupiedPlaces"
  }
}

class ParkingsRepo {
  private(set) var list = [Parking]()
  private(set) var isFailed = false

  var onLoaded: (() -> Void)?

  func load() {
    let cameras = Constants.baseURL.appendingPathComponent("info")
    URLSession.shared.dataTask(with: cameras) { (data, response, error) in
      defer {
        self.onLoaded?()
      }

      if error != nil {
        self.isFailed = true
        return
      }

      guard let data = data else {
        self.isFailed = true
        return
      }

      if let listItems = try? JSONDecoder().decode([Parking].self, from: data) {
        self.list = listItems
      } else {
        self.isFailed = true
      }
    }.resume()
  }
}

class ParkingsListViewController: UITableViewController {

  let parkings = ParkingsRepo()

  override func viewDidLoad() {
    super.viewDidLoad()

    parkings.onLoaded = { [unowned self] in
      DispatchQueue.main.async {
        self.tableView.refreshControl?.endRefreshing()
        self.updateState()
      }
    }
    parkings.load()
    tableView.refreshControl = UIRefreshControl()
    tableView.refreshControl?.addTarget(self, action: #selector(refresh), for: .valueChanged)
  }

  func updateState() {
    if parkings.isFailed {
      //TODO: show error
    } else {
      tableView.reloadData()
    }
  }

  @objc private func refresh() {
    parkings.load()
  }

  // MARK: - Table view data source

  override func numberOfSections(in tableView: UITableView) -> Int {
    return 1
  }

  override func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    return 79.0
  }

  override func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    return parkings.list.count
  }

  override func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    let cell = tableView.dequeueReusableCell(withIdentifier: "parkCell", for: indexPath)
    guard let parkCell = cell as? ParkCell else { return cell }
    parkCell.set(parkings.list[indexPath.row])
    return cell
  }

  override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
    if segue.identifier == "details" {
      guard let details = segue.destination as? ViewController else { return }
      if let selectedIndex = tableView.indexPathForSelectedRow,
        selectedIndex.row < parkings.list.count {
        details.parking = parkings.list[selectedIndex.row]
      }
    }
  }
}
