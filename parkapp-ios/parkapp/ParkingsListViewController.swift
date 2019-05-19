//
//  ParkingsListViewController.swift
//  parkapp
//
//  Created by Ilya Sedov on 19/05/2019.
//  Copyright © 2019 SKB Kontur. All rights reserved.
//

import UIKit

class ParkingsListViewController: UITableViewController {

  let parkings = CamsRepo.shared
  private var disposer = [AnyObject]()

  override func viewDidLoad() {
    super.viewDidLoad()

    disposer.append(
      NotificationCenter.default
        .addObserver(forName: CamsRepo.allLoaded, object: CamsRepo.shared, queue: OperationQueue.main) { (_) in
          self.tableView.refreshControl?.endRefreshing()
          self.updateState()
      })
    disposer.append(
      NotificationCenter.default
        .addObserver(forName: CamsRepo.diff, object: CamsRepo.shared, queue: OperationQueue.main, using: { (_) in
          self.updateState()
    }))

    tableView.refreshControl = UIRefreshControl()
    tableView.refreshControl?.addTarget(self, action: #selector(refresh), for: .valueChanged)
  }

  func updateState() {
//    if parkings.isFailed {
//      //TODO: show error
//    } else {
      tableView.reloadData()
//    }
  }

  @objc private func refresh() {
    parkings.refresh()
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
